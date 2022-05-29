namespace IllusionScript.Compiler.BCC
{
    public enum KeywordCollection
    {
        HeadStart,
        ItemEnd,
        Label,
        Goto,
        ConditionalGoto,
        
        Expression,
        LiteralInt,
        LiteralBool,
        EndString,
        LiteralString,
        
        Call,
        Assign,
        Bin,
        Un,
        EndCall,
        
        SepSplit,
        Return,
        Const,
        Let,
        Negation,
        
        Identity,
        LogicalNegation,
        OnesComplement,
        Addition,
        Subtraction,
        
        Multiplication,
        Division,
        Modulo,
        Pow,
        LogicalAnd,
        
        LogicalOr,
        NotEquals,
        Equals,
        BitwiseAnd,
        BitwiseOr,
        
        BitwiseXor,
        BitwiseShiftLeft,
        BitwiseShiftRight,
        Less,
        LessEquals,
        
        Greater,
        GreaterEquals,
        CloseCommand,
        
        // Reserved names for static functions
        Syscall
    }
}