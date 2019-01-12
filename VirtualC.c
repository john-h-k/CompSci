// ObjectivelyBadC.c : This file contains the 'main' function. Program execution begins and ends there.
//

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <assert.h>

#define CLASS_HEADER Base header;
#define CLASS_INHERITS(PARENT) PARENT parent;
#define BASE(X) (*(Base*)&X)
#define BASE_PTR(X) ((Base*)&X)
#define STUB(X) ((Stub)X)
#define STUB_PTR(X) ((Stub*)&X)
#define UP_CAST(X, TYPE) (*(TYPE*)&X)
#define UP_CAST_PTR(X, TYPE) ((TYPE*)&X)
#define REFLECTIVE_FUNC(F)
#define ANY_ARGS

struct Base;
void ConstructVTableForTypeNoParent(struct Base* pClass, size_t numMethods);

#define CREATE_INIT_NO_PARENT(X, METHOD_NUM) ConstructVTableForTypeNoParent(BASE_PTR(X), METHOD_NUM);

typedef int slot;
typedef void(*Stub) (ANY_ARGS);
typedef struct _VTable
{
	size_t size;
	Stub* methods;
} VTable;

typedef struct _Base
{
	VTable table;
} Base;

void ConstructVTableForTypeNoParent(Base* pClass, size_t numMethods)
{
	pClass->table.size = numMethods;
	pClass->table.methods = (Stub*)malloc(numMethods * sizeof(Stub));
}

void ConstructVTableForTypeWithParent(Base* pEmptyChild, Base* pParentType, size_t newNumMethods, size_t sizeOfParent)
{
	const size_t size = pParentType->table.size + newNumMethods;
	ConstructVTableForTypeNoParent(pEmptyChild, size);
	memcpy(pEmptyChild, pParentType, sizeOfParent);
	pEmptyChild->table.size = size;
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
	printf("Parent age: %d\n", p.age);
}

void PrintChild(Parent p)
{
	printf("Child age: %d\n", p.age);
}

void PrintDerived(Parent p)
{
	printf("Derived age: %d\n", p.age);
}

int main(void)
{
	printf("Testing virtual table creation...\n");

	Parent p;
	p.age = 100;
	ConstructVTableForTypeNoParent(BASE_PTR(p), 1);
	SetOrOverrideMethodAndBind(BASE_PTR(p), 0, PrintParent, STUB_PTR(p.speak));
	p.speak(p);

	// Child
	Child c;
	ConstructVTableForTypeWithParent(BASE_PTR(c), BASE_PTR(p), 0, sizeof(Parent));
	SetOrOverrideMethodAndBindMirror(BASE_PTR(p), 0, PrintChild, STUB_PTR(c.parent.speak), STUB_PTR(c.speak));
	c.parent.age = 10;
	c.speak(c);

	// Derived
	Derived d;
	ConstructVTableForTypeWithParent(BASE_PTR(d), BASE_PTR(c), 0, sizeof(Child));
	SetOrOverrideMethodAndBindMirror(BASE_PTR(c), 0, PrintDerived, STUB_PTR(d.parent.parent.speak), STUB_PTR(d.speak));
	d.parent.parent.age = 1;
	d.speak(d);

	assert((char*)&p + sizeof(p) <= (char*)&c 
		&& (char*)&c + sizeof(d) <= (char*)&d);

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
