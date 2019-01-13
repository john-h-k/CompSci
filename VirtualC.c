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
#define THIS(NAME) NAME*
#define REFLECTIVE_FUNC(NAME)
#define ANY_ARGS
#define NEW(ID) { .header = GetVTableForType(ID) }

typedef int slot;
typedef int TypeId;
typedef void(*Stub) (ANY_ARGS);
typedef struct _Base Base;

//#define CONSTRUCT(TYPE, METHOD_NUM) (TYPE) GetVTableForType(GetNextTypeId(), METHOD_NUM); TODO
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

static TypeId __currentId = 0;
static int reSizeNum = 1 << 8;
static Base* tables = NULL;

TypeId GetNextTypeId() { return __currentId++;  }

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

typedef void(*BindFunc)(TypeId id, void* class);
static BindFunc* bindingFunctions;
static int funcReSizeNum = 1 << 8;

void EnsureFuncArraySize()
{
	if (bindingFunctions == NULL)
	{
		bindingFunctions = calloc(reSizeNum, sizeof(Base));
	}
	if (__currentId == reSizeNum)
	{
		funcReSizeNum <<= 1;
		bindingFunctions = realloc(tables, sizeof(Base) * reSizeNum);
		memset(tables + reSizeNum, 0, reSizeNum * sizeof(Base));
	}
}

void SetDefaultBindForTypeId(TypeId id, BindFunc bind)
{
	EnsureFuncArraySize();
	bindingFunctions[id] = bind;
}

void BindTableFromDefault(TypeId id, void* class)
{
	bindingFunctions[id](id, class);
}

bool DoesVTableExistForType(TypeId id)
{
	if (*(int*)&tables[id] != -1) return true;
	return false;
}

Base GetVTableForType(TypeId id)
{
	EnsureTableArraySize();
	return tables[id];
}

Base NewVTableForType(TypeId id, size_t numMethods)
{

	EnsureTableArraySize();
	tables[id].table.methods = malloc(sizeof(Stub) * numMethods);
	tables[id].table.size = numMethods;
	return tables[id];
}

void SetVTableForType(TypeId id, Base table)
{
	EnsureTableArraySize();
	if (!DoesVTableExistForType(id)) NewVTableForType(id, table.table.size);
	tables[id] = table;
}

void DestroyVTableForType(TypeId id)
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

void SetTable(TypeId id, const Base* base, Base* class)
{
	SetVTableForType(id, *base);
	memcpy(class, &base, sizeof(Base));
}

void CallVirtualNoArgs(Base* pClass, slot methodSlot)
{
	pClass->table.methods[methodSlot]();
}

Base __doNotModifyGlobalForMacro;
#define NEW_BIND(TYPE, ID) (bindingFunctions[ID](ID, &__doNotModifyGlobalForMacro), *(TYPE*)&__doNotModifyGlobalForMacro)
typedef struct _Parent
{
	CLASS_HEADER
	int age;
	void(*speak)(THIS(struct _Parent), ...);
} Parent;

typedef struct _Child
{
	CLASS_INHERITS(Parent)
	REFLECTIVE_FUNC("Parent.speak") void(*speak)(THIS(struct _Child), ...);
} Child;

typedef struct _Derived
{
	CLASS_INHERITS(Child)
	REFLECTIVE_FUNC("Child.speak") void(*speak)(THIS(struct _Derived), ...);
} Derived;

void PrintParent(Parent p, const char* CallingName)
{
	printf("Called from Parent - age: %d\n", p.age);
	printf("Calling name is: %s\n", CallingName);
}

void PrintChild(Parent p, const char* CallingName)
{
	printf("Called from Child - age: %d\n", p.age);
	printf("Calling name is: %s\n", CallingName);
}

void PrintDerived(Parent p, const char* CallingName)
{
	printf("Called from Derived - age: %d\n", p.age);
	printf("Calling name is: %s\n", CallingName);
}

void BindParent(TypeId id, void* class)
{
	Parent* p = (Parent*)class;
	p->header = GetVTableForType(id);
	p->speak = (void*)GetVTableForType(id).table.methods[0];

}

void BindChild(TypeId id, void* class)
{
	Child* p = (Child*)class;
	p->parent.header = GetVTableForType(id);
	p->speak = (void*)GetVTableForType(id).table.methods[0];
	p->parent.speak = (void*)p->speak;
}

void BindDerived(TypeId id, void* class)
{
	Derived* p = (Derived*)class;
	p->parent.parent.header = GetVTableForType(id);
	p->speak = (void*)GetVTableForType(id).table.methods[0];
	p->parent.speak = (void*)p->speak;
	p->parent.parent.speak = (void*)p->speak;
}

void SetTablesForType(TypeId parentId, TypeId childId, TypeId derivedId)
{
	Base pBase = NewVTableForType(parentId, 1);
	SetOrOverrideMethod(&pBase, 0, PrintParent);
	SetVTableForType(parentId, pBase);

	Base cBase = NewVTableForType(childId, 1);
	memcpy(&cBase, &pBase, sizeof(Base));
	SetOrOverrideMethod(&cBase, 0, PrintChild);
	SetVTableForType(childId, cBase);

	Base dBase = NewVTableForType(derivedId, 1);
	memcpy(&dBase, &cBase, sizeof(Child));
	SetOrOverrideMethod(&dBase, 0, PrintDerived);
	SetVTableForType(derivedId, dBase);
}

int main(void)
{
	printf("Testing virtual table creation...\n");

#pragma region INIT

	const TypeId parentId = GetNextTypeId();
	const TypeId childId = GetNextTypeId();
	const TypeId derivedId = GetNextTypeId();

	SetTablesForType(parentId, childId, derivedId);

	SetDefaultBindForTypeId(parentId, BindParent);
	SetDefaultBindForTypeId(childId, BindChild);
	SetDefaultBindForTypeId(derivedId, BindDerived);
#pragma endregion INIT

	// Parent
	Parent p = NEW_BIND(Parent, parentId);
	p.age = 100;
	p.speak(&p, "Hi, this string has been passed through an un-prototyped function from parent");

	// Child
	Child c = NEW_BIND(Child, childId);
	c.parent.age = 10;
	c.speak(&c, "Hi, this string has been passed through an un-prototyped function from child");

	// Derived
	Derived d = NEW_BIND(Derived, derivedId);
	d.parent.parent.age = 1;
	d.speak(&d, "Hi, this string has been passed through an un-prototyped function from derived");

	printf("Testing polymorphism from highest level [Parent]...\n");

	Parent* pParentCastParent = UP_CAST_PTR(p, Parent);
	pParentCastParent->speak(pParentCastParent, "Hi, this string has been passed through an un-prototyped function from parent, as a Parent*");
	Parent* pChildCastParent = UP_CAST_PTR(c, Parent);
	pChildCastParent->speak(&UP_CAST(c, Parent), "Hi, this string has been passed through an un-prototyped function from child, as a Parent*");
	Parent* pDerivedCastParent = UP_CAST_PTR(d, Parent);
	pDerivedCastParent->speak(&UP_CAST(d, Parent), "Hi, this string has been passed through an un-prototyped function from derived, as a Parent*");

	printf("Testing polymorphism from second level [Child]...\n");

	Child* pChildCastChild = UP_CAST_PTR(c, Child);
	pChildCastParent->speak(UP_CAST_PTR(c, Parent), "Hi, this string has been passed through an un-prototyped function from child, as a Child*");
	Child* pDerivedCastChild = UP_CAST_PTR(d, Child);
	pDerivedCastParent->speak(UP_CAST_PTR(d, Parent), "Hi, this string has been passed through an un-prototyped function from derived, as a Child*");

	return 0;
}
