#include <stdio.h>
#include <string.h>
#include <stdlib.h>

typedef int slot;
typedef void (*Stub) (void);
typedef struct _VTable
{
  size_t size;
  Stub* methods;
} VTable;

typedef struct _Class
{
  VTable table;
} Class;

void ConstructVTableForTypeNoParent(Class* pClass, size_t numMethods)
{
  pClass->table.size = 0;
  pClass->table.methods = (Stub*)malloc(numMethods * sizeof(Stub));
}

void ConstructVTableForTypeWithParent(Class* pChild, Class* pParent, size_t newNumMethods)
{
  size_t size = pParent->table.size;
  ConstructVTableForTypeNoParent(pChild, newNumMethods + size);
  memcpy(pParent->table.methods, pChild->table.methods, size * sizeof(Stub));
}

void SetOrOverrideMethod(Class* pType, slot methodSlot, void* pNewMethodPtr)
{
  pType->table.methods[methodSlot] = (Stub)pNewMethodPtr;
  pType->table.size++;
}

void CallVirtual(Class* pClass, slot methodSlot)
{
  pClass->table.methods[methodSlot]();
}

typedef struct _Parent
{
  VTable vtb;
} Parent;

typedef struct _Child
{
  VTable vtb;
} Child;

void PrintParent()
{
  printf("Parent\n");
}

void PrintChild()
{
  printf("Child\n");
}

int main(void) 
{
  Parent p;
  ConstructVTableForTypeNoParent((Class*)&p, 1);
  SetOrOverrideMethod((Class*)&p, 0, PrintParent);
  CallVirtual((Class*)&p, 0);
  // Child
  Child c;
  ConstructVTableForTypeWithParent((Class*)&c, (Class*)&p, 0);
  SetOrOverrideMethod((Class*)&c, 0, PrintChild);
  CallVirtual((Class*)(Parent*)&c, 0);
  return 0;
}

