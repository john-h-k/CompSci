.assembly extern mscorlib {}
.assembly HelloWorld {}
.module HelloWorld.exe

.class private abstract sealed auto ansi beforefieldinit Hello.Program extends [mscorlib]System.Object
{
    .method private static hidebysig void Complain() cil managed
    {
        ldstr "Not a valid base 10 number"
        call void [mscorlib]System.Console::WriteLine(string)
        call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
        pop // Clean the stack
        ret
    }

    .method private static hidebysig void Main() cil managed
    {
        .entrypoint

        // Initialize 2 strings and 2 integers
        .locals init 
        (
            [0] string firstRequest,
            [1] string secondRequest,
            [2] int32 firstNumber,
            [3] int32 secondNumber
        )

        // Load 2 strings into the string references
        ldstr "Enter a number:"
        stloc firstRequest
        ldstr "Enter another number:"
        stloc secondRequest

        // Request, read, and parse the first number
        ldloc firstRequest
        call void [mscorlib]System.Console::WriteLine(string)
        call string [mscorlib]System.Console::ReadLine()
        ldloca.s firstNumber // Load the address of the first int to the stack
        call bool [mscorlib]System.Int32::TryParse(string, int32&)
        brfalse.s COMPLAIN

        // Repeat for the second
        ldloc secondRequest
        call void [mscorlib]System.Console::WriteLine(string)
        call string [mscorlib]System.Console::ReadLine()
        ldloca.s secondNumber
        call bool [mscorlib]System.Int32::TryParse(string, int32&)
        brfalse.s COMPLAIN


        // Print
        ldstr "The sum of those 2 numbers is: "
        call void [mscorlib]System.Console::Write(string)

        // Load the 2 numbers to the stack, add them, and print them
        ldloc firstNumber
        ldloc secondNumber
        add
        call void [mscorlib]System.Console::WriteLine(int32)
        call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
        pop
        br.s RETURN

        // Possibly complain, then return
        COMPLAIN: call void Hello.Program::Complain()
        RETURN: ret
    }
}
