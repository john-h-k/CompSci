.assembly extern mscorlib {}
.assembly FastStructureCopy {}
.module FastStructureCopy.dll

.class public abstract sealed auto ansi FastStructureCopy.Copy
extends [mscorlib]System.Object
{
    .method public hidebysig static uint8[]
    CopyStruct<valuetype .ctor (class [mscorlib]System.ValueType modreq ([System.Runtime.InteropServices]System.Runtime.InteropServices.UnmanagedType)) T>(
        !!T 'val'
    )
    cil managed aggressiveinlining
    {
        .param type [1] /*T*/
        .custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
         = (01 00 00 00 )
        
        .locals init (
            uint8[] pinned 'array',
            int32 'size'
        )

        .maxstack 3
        sizeof !!0 // get size of type - cannot be 0 - stack depth 1 (size)
        dup // - stack depth 2 (size, size)
        stloc 'size' // store it - stack depth 1 (size)
        
        newarr [mscorlib]System.Byte // make an array from it - guaranteed not null - stack depth 1 (array ref)

        dup // - stack depth 2 (array ref, array ref)
        stloc 'array' // - stack depth 1 (array ref)

        ldc.i4.0 // - stack depth 2 (int 0, array ref)
        ldelema [mscorlib]System.Byte // get first elem address (index 0) - stack depth 1 (first elem address)
        conv.u // convert to pointer - stack depth 1 (first elem pointer)
        
        ldarga.s 'val' // load address of 'val' unmanaged type - stack depth 2 (val pointer, first elem pointer)
        ldloc 'size' // - stack depth 3 (size, val pointer, first elem pointer)
        cpblk // - stack depth 0

        ldloc 'array' // - stack depth 1 (array ref)
        ret
    }

    .method public hidebysig static uint8[]
    CopyStructRef<valuetype .ctor (class [mscorlib]System.ValueType modreq ([System.Runtime.InteropServices]System.Runtime.InteropServices.UnmanagedType)) T>(
        [in] !!T& 'val'
    )
    cil managed aggressiveinlining
    {
        .param type [1]
        .custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
         = (01 00 00 00 )
         .param [1]
        .custom instance void [mscorlib]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor() 
        = ( 01 00 00 00 )

        .locals init (
            uint8[] pinned 'array',
            int32 'size'
        )

        .maxstack 3
        sizeof !!0 // get size of type - cannot be 0 - stack depth 1 (size)
        dup // - stack depth 2 (size, size)
        stloc 'size' // store it - stack depth 1 (size)
        
        newarr [mscorlib]System.Byte // make an array from it - guaranteed not null - stack depth 1 (array ref)

        dup // - stack depth 2 (array ref, array ref)
        stloc 'array' // - stack depth 1 (array ref)

        ldc.i4.0 // - stack depth 2 (int 0, array ref)
        ldelema [mscorlib]System.Byte // get first elem address (index 0) - stack depth 1 (first elem address)
        conv.u // convert to pointer - stack depth 1 (first elem pointer)
        
        ldarga.s 'val' // load address of 'val' unmanaged type - stack depth 2 (val pointer, first elem pointer)
        ldloc 'size' // - stack depth 3 (size, val pointer, first elem pointer)
        cpblk // - stack depth 0

        ldloc 'array' // - stack depth 1 (array ref)
        ret
    }

    .method public hidebysig static void
    CopyStruct<valuetype .ctor (class [mscorlib]System.ValueType modreq ([System.Runtime.InteropServices]System.Runtime.InteropServices.UnmanagedType)) T>(
        !!T 'val',
        uint8* 'start'
    )
    cil managed aggressiveinlining
    {
        .param type [1] /*T*/
        .custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
         = (01 00 00 00 )

        .maxstack 3
        ldarg.1 // - stack depth 1 (dest pointer)
        ldarga.s 0 // - stack depth 2 (src pointer, dest pointer)
        sizeof !!T // - stack depth 2 (size, src pointer, dest pointer)
        cpblk
        // clean stack
        ret
    }

    .method public hidebysig static void
    CopyStructRef<valuetype .ctor (class [mscorlib]System.ValueType modreq ([System.Runtime.InteropServices]System.Runtime.InteropServices.UnmanagedType)) T>(
        [in] !!T& 'val',
        uint8* 'start'
    )
    cil managed aggressiveinlining
    {
        .param type [1]
        .custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
         = (01 00 00 00 )
         .param [1]
        .custom instance void [mscorlib]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor() 
        = ( 01 00 00 00 )

        .maxstack 3
        ldarg.1 // - stack depth 1 (dest pointer)
        ldarg.0 // - stack depth 2 (src ref [in], dest pointer)
        conv.u // src ref as src pointer - stack depth 2 (src pointer, dest pointer)
        sizeof !!T // - stack depth 2 (size, src pointer, dest pointer)
        cpblk
        // clean stack
        ret
    }
}

.class private auto ansi sealed beforefieldinit Microsoft.CodeAnalysis.EmbeddedAttribute
    extends [mscorlib]System.Attribute
{
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
        01 00 00 00
    )
    .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = (
        01 00 00 00
    )
    // Methods
    .method public hidebysig specialname rtspecialname 
        instance void .ctor () cil managed 
    {
        // Method begins at RVA 0x2050
        // Code size 7 (0x7)
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: call instance void [mscorlib]System.Attribute::.ctor()
        IL_0006: ret
    } // end of method EmbeddedAttribute::.ctor

} // end of class Microsoft.CodeAnalysis.EmbeddedAttribute

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.IsUnmanagedAttribute
    extends [mscorlib]System.Attribute
{
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
        01 00 00 00
    )
    .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = (
        01 00 00 00
    )
    // Methods
    .method public hidebysig specialname rtspecialname 
        instance void .ctor () cil managed 
    {
        // Method begins at RVA 0x2050
        // Code size 7 (0x7)
        .maxstack 8

        IL_0000: ldarg.0
        IL_0001: call instance void [mscorlib]System.Attribute::.ctor()
        IL_0006: ret
    } // end of method IsUnmanagedAttribute::.ctor

} // end of class System.Runtime.CompilerServices.IsUnmanagedAttribute