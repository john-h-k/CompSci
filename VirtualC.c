// ObjectivelyBadC.c : This file contains the 'main' function. Program execution begins and ends there.
//

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdbool.h>

#define CLASS_HEADER Base header;
#define CLASS_INHERITS(PARENT) PARENT parent;
#define BASE(X) (*(Base*)&X)
#define BASE_PTR(X) ((Base*)&X)
#define STUB(X) ((Stub)X)
#define STUB_PTR(X) ((Stub*)&X)
#define UP_CAST(X, TYPE) (*(TYPE*)&X)
#define UP_CAST_PTR(X, TYPE) ((TYPE*)&X)
#define REFLECTIVE_FUNC(NAME)
#define ANY_ARGS

typedef int slot;
typedef int TableId;
typedef void(*Stub) (ANY_ARGS);
typedef struct _Base Base;

Base GetVTableForType(TableId id, size_t numMethods);



//#define CONSTRUCT(TYPE, METHOD_NUM) (TYPE) GetVTableForType(GetNextTableId(), METHOD_NUM); TODO
#define GET_TABLE(TYPE, METHOD_NUM) GetVTableForType(GetNextTableId(), METHOD_NUM)
#define SET_TABLE(ID, TABLE, CLASS) SetVTableForType(ID, TABLE);\
AssignTable(&CLASS, &TABLE)

typedef struct _VTable
{
	size_t size;
	Stub* methods;
} VTable;

typedef struct _Base
{
	VTable table;
} Base;

static TableId __currentId = 0;
static int reSizeNum = 1 << 8;
static Base* tables = NULL;

TableId GetNextTableId() { return __currentId++;  }

void AssignTable(void* class, Base* table)
{
	memcpy(class, table, sizeof(Base));
}

void EnsureTableArraySize()
{
	if (tables == NULL)
	{
		tables = malloc(reSizeNum * sizeof(Base));
		memset(tables, -1, reSizeNum * sizeof(Base));
	}
	if (__currentId == reSizeNum)
	{
		reSizeNum <<= 1;
		tables = realloc(tables, sizeof(Base) * reSizeNum);
		memset(tables + reSizeNum, -1, reSizeNum * sizeof(Base));
	}
}

bool DoesVTableExistForType(TableId id)
{
	if (*(int*)&tables[id] != -1) return true;
	return false;
}

Base GetVTableForType(TableId id)
{
	EnsureTableArraySize();
	return tables[id];
}

Base NewVTableForType(TableId id, size_t numMethods)
{

	EnsureTableArraySize();
	tables[id].table.methods = malloc(sizeof(Stub) * numMethods);
	tables[id].table.size = numMethods;
	return tables[id];
}

void SetVTableForType(TableId id, Base table)
{
	tables[id] = table;
}

void DestroyVTableForType(TableId id)
{
	free(tables[id].table.methods);
	tables[id].table.size = 0;
}

void DeconstructVTable(Base* pClass)
{
	free(pClass->table.methods);
}


void SetOrOverrideMethod(Base* pType, slot methodSlot, Stub pNewMethodPtr)
{
	pType->table.methods[methodSlot] = (Stub)pNewMethodPtr;
}

void SetOrOverrideMethodAndBind(Base* pType, slot methodSlot, Stub pNewMethodPtr, Stub* ppFunc)
{
	pType->table.methods[methodSlot] = pNewMethodPtr;
	*ppFunc = pType->table.methods[methodSlot];
}

void SetOrOverrideMethodAndBindMirror(Base* pType, slot methodSlot, Stub pNewMethodPtr, Stub* ppFunc, Stub* ppChildBind)
{
	pType->table.methods[methodSlot] = pNewMethodPtr;
	*ppFunc = pType->table.methods[methodSlot];
	*ppChildBind = pType->table.methods[methodSlot];
	
}

void CallVirtualNoArgs(Base* pClass, slot methodSlot)
{
	pClass->table.methods[methodSlot]();
}

typedef struct _Parent
{
	CLASS_HEADER
	void(*speak)(struct _Parent p);
	int age;
} Parent;

typedef struct _Child
{
	CLASS_INHERITS(Parent)
	REFLECTIVE_FUNC("Parent.speak") void(*speak)(struct _Child p);
} Child;

typedef struct _Derived
{
	CLASS_INHERITS(Child)
	REFLECTIVE_FUNC("Child.speak") void(*speak)(struct _Derived p);
} Derived;

void PrintParent(Parent p)
{
	printf("Called from Parent - age: %d\n", p.age);
}

void PrintChild(Parent p)
{
	printf("Called from Child - age: %d\n", p.age);
}

void PrintDerived(Parent p)
{
	printf("Called from Derived - age: %d\n", p.age);
}

int main(void)
{
	printf("Testing virtual table creation...\n");

	// Parent
	Parent p;
	const TableId parentId = GetNextTableId();
	Base pTable = NewVTableForType(parentId, 0);
	SetOrOverrideMethodAndBind(&pTable, 0, PrintParent, STUB_PTR(p.speak));
	SET_TABLE(parentId, pTable, p);
	p.age = 100;
	p.speak(p);

	// Child
	Child c;
	const TableId childId = GetNextTableId();
	Base cTable = GetVTableForType(parentId);
	SetOrOverrideMethodAndBindMirror(&pTable, 0, PrintChild, STUB_PTR(c.parent.speak), STUB_PTR(c.speak));
	SET_TABLE(childId, cTable, c);
	c.parent.age = 10;
	c.speak(c);

	// Derived
	Derived d;
	const TableId derivedId = GetNextTableId();
	Base dTable = GetVTableForType(childId);
	SetOrOverrideMethodAndBindMirror(&cTable, 0, PrintDerived, STUB_PTR(d.parent.parent.speak), STUB_PTR(d.speak));
	SET_TABLE(derivedId, dTable, d);
	d.parent.parent.age = 1;
	d.speak(d);

	printf("Testing polymorphism from highest level [Parent]...\n");

	Parent* pParentCastParent = UP_CAST_PTR(p, Parent);
	pParentCastParent->speak(p);
	Parent* pChildCastParent = UP_CAST_PTR(c, Parent);
	pChildCastParent->speak(UP_CAST(c, Parent));
	Parent* pDerivedCastParent = UP_CAST_PTR(d, Parent);
	pDerivedCastParent->speak(UP_CAST(d, Parent));

	printf("Testing polymorphism from second level [Child]...\n");

	Child* pChildCastChild = UP_CAST_PTR(c, Child);
	pChildCastParent->speak(UP_CAST(c, Parent));
	Child* pDerivedCastChild = UP_CAST_PTR(d, Child);
	pDerivedCastParent->speak(UP_CAST(d, Parent));

	return 0;
}
