using System.Runtime.InteropServices.JavaScript;
using WaffleEngine;

namespace MagicGame.Scripts;

public class VirtualMachine
{
    public List<Func<Stack<int>, bool>> ExternalFunctions;
    public Stack<int> Stack = new Stack<int>(1024);
    public Stack<int> CallStack = new Stack<int>(64);
    public Program? Program;
    public bool ProgramFinished;
    public int InstructionPtr = 0;

    public VirtualMachine(List<Func<Stack<int>, bool>> externalFunctions)
    {
        ExternalFunctions = externalFunctions;
    }
    
    public void SetProgram(Program program, int entry)
    {
        Program = program;
        ProgramFinished = false;
        InstructionPtr = entry;
        Stack.Clear();
    }
    
    public InstructionResult Step()
    {
        if (Program is null || ProgramFinished)
            return InstructionResult.Error(-1);

        if (InstructionPtr >= Program.Instructions.Count)
        {
            Return();
            
            if (ProgramFinished)
                return InstructionResult.Success(InstructionPtr);
        }

        if (!TryExecuteInstruction(Program.Instructions[InstructionPtr]))
        {
            ProgramFinished = true;
            return InstructionResult.Error(InstructionPtr);
        }

        InstructionPtr += 1;
        return InstructionResult.Success(InstructionPtr - 1);
    }

    private bool TryExecuteInstruction(Instruction instruction)
    {
        Start:
        switch (instruction.Type)
        {
            case Operation.Pop:
                // Pop X from stack
                for (int i = 0; i < instruction.Data; i++)
                {
                    // If trying to pop too many fail.
                    if (Stack.Count == 0)
                        return false;

                    Stack.Pop();
                }
                return true;
            case Operation.Push:
                // If Stack gets too big fail.
                if (Stack.Count + 1 > 1024)
                    return false;
                // Push whatever data to stack.
                Stack.Push(instruction.Data);
                return true;
            case Operation.Call:
                return RunCall(instruction.Data, InstructionPtr);
            case Operation.CallExternal:
                // Check external function exists
                if (instruction.Data < 0 || instruction.Data >= ExternalFunctions.Count)
                    return false;
                // Call the external function.
                return ExternalFunctions[instruction.Data].Invoke(Stack);
            case Operation.If:
                // Fail if nothing on stack.
                if (!Stack.TryPeek(out var result))
                    return false;
                // If result isn't 0 call supplied address.
                if (result != 0)
                {
                    return RunCall(instruction.Data, InstructionPtr);
                }
                return true;
            case Operation.While:
                // Fail if nothing on stack.
                if (!Stack.TryPeek(out var whileResult))
                    return false;
                // If result isn't 0 call supplied address.
                if (whileResult != 0)
                {
                    // Return to the instruction before this
                    // as it'll then keep running the condition until false.
                    return RunCall(instruction.Data, InstructionPtr - 1);
                }
                return true;
            case Operation.Block:
                Return();
                
                if (ProgramFinished)
                {
                    return true;
                }
                
                // Uses goto to stop creating extra stack frames
                // and having to wait for the next step.
                goto Start;
            case Operation.Error:
                return false;
            default: 
                return false;
        }
    }

    private bool RunCall(int callTo, int returnTo)
    {
        // If Call stack gets too big fail.
        if (CallStack.Count + 1 > 64)
            return false;
        // Push the op to return to.
        CallStack.Push(returnTo);
        // Update the instruction ptr.
        InstructionPtr = callTo;
        return true;
    }

    private void Return()
    {
        // If CallStack is empty the program is finished.
        if (CallStack.Count == 0)
        {
            ProgramFinished = true;
            return;
        }
        // Otherwise process the next op.
        InstructionPtr = CallStack.Pop() + 1;
    }

    public void PrintCurrent()
    {
        if (Program is null || Program?.Instructions.Count <= InstructionPtr)
            return;
        
        Log.Info($"{Program!.Instructions[InstructionPtr].Type}");
    }

    public void PrintStack()
    {
        Log.Info("-- Stack --");

        foreach (var (index, value) in Stack.Index())
        {
            Log.Info($"{index} - {value}");
        }
    }
}

public class Program
{
    public Dictionary<string, int> BlockLocations;
    public List<Instruction> Instructions;

    public Program(List<Instruction> instructions, Dictionary<string, int> blockLocations)
    {
        Instructions = instructions;
        BlockLocations = blockLocations;
    }
}

public struct Instruction
{
    public Operation Type;
    public int Data;
}

public enum Operation
{
    Block,
    Pop,
    Push,
    Call,
    CallExternal,
    While,
    If,
    Error,
}

public struct InstructionResult
{
    public bool IsError;
    public int Index;

    public static InstructionResult Error(int index) => new InstructionResult()
    {
        IsError = true,
        Index = index,
    };
    
    public static InstructionResult Success(int index) => new InstructionResult()
    {
        IsError = false,
        Index = index,
    };
}