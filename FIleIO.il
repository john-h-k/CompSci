.assembly extern mscorlib {}
.assembly FileIO {}
.module FileIO.exe

.class private abstract sealed auto ansi FileIO.Program 
extends [mscorlib]System.Object
{
    .method private static hidebysig int32 Main() cil managed
    {
        .entrypoint

        ldstr "Opening file Test.txt..."
        call void [mscorlib]System.Console::WriteLine(string)


        ldstr "Test.txt"
        ldc.i4 3
        newobj instance void [mscorlib]System.IO.FileStream::.ctor(string, valuetype [mscorlib]System.IO.FileMode)
        newobj instance void [mscorlib]System.IO.StreamReader::.ctor(class [mscorlib]System.IO.Stream) 
        callvirt instance string [mscorlib]System.IO.StreamReader::ReadToEnd()
        call void [mscorlib]System.Console::WriteLine(string)
        call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
        pop
        ldc.i4.0
        ret
    }
}