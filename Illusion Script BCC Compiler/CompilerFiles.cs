using System;
using System.Collections.Immutable;
using System.IO;
using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Binding.Operators;
using IllusionScript.Runtime.Compiling;
using IllusionScript.Runtime.Interpreting.Memory.Symbols;

namespace IllusionScript.Compiler.BCC
{
    public class CompilerFiles : CompilerWriter
    {
        private AddressManager addressManager;
        private FunctionSymbol function;

        public CompilerFiles(FileStream writer) : base(writer)
        {
            addressManager = new AddressManager();
        }

        public void Write(FunctionSymbol function,
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> bodies)
        {
            this.function = function;
            WriteHeader();
            WriteBlockStatement(bodies[function]);
        }

        private void WriteHeader()
        {
            /*
                -- header
                8bit keyword
                8bit = name
                8bit = parameter count
                16bit = parameter pair
                    - 8bit name
                    - 8bit type
                8bit = return type

                # min 32bit header
                # 32bit + parameter size
            */

            writer.WriteBytes(ToByte(KeywordCollection.HeadStart, 8));
            int[] nameAddress = addressManager.get(function.name);

            byte[] nameBytes = ToByte(nameAddress, 8);
            writer.WriteBytes(nameBytes);

            byte[] parameterCount = ToByte(function.parameters.Length, 8);
            writer.WriteBytes(parameterCount);

            foreach (ParameterSymbol parameter in function.parameters)
            {
                int[] parameterAddress = addressManager.get(parameter.name);
                byte[] parameterPair = ToByte(parameterAddress, 8);
                writer.WriteBytes(parameterPair);

                int[] typeKeyword = addressManager.get(parameter.type.name);
                byte[] typeKeywordBytes = ToByte(typeKeyword, 8);
                writer.WriteBytes(typeKeywordBytes);
            }

            int[] returnType = addressManager.get(function.returnType.name);
            byte[] returnTypeBytes = ToByte(returnType, 8);
            writer.WriteBytes(returnTypeBytes);
        }

        private byte[] ToByte(int[] ints, int length)
        {
            byte[] result = new byte[length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = 0;
            }

            for (var i = 0; i < ints.Length; i++)
            {
                result[i] = Convert.ToByte(ints[i]);
            }

            return result;
        }

        private byte[] ToByte(int i, int length)
        {
            return ToByte(new[] { i }, length);
        }

        private byte[] ToByte(KeywordCollection item, int length)
        {
            return ToByte(new[] { (int)item + 1 }, length);
        }

        private byte[] GenerateEmpyt(int length)
        {
            byte[] bytes = new byte[length];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }

            return bytes;
        }

        public void Close()
        {
            writer.Close();
        }

        protected override void WriteLabelStatement(BoundLabelStatement statement)
        {
            byte[] keyWordBytes = ToByte(KeywordCollection.Label, 8);
            writer.WriteBytes(keyWordBytes);

            int[] label = addressManager.get(statement.BoundLabel.name);
            byte[] labelBytes = ToByte(label, 8);
            writer.WriteBytes(labelBytes);
        }

        protected override void WriteConditionalGotoStatement(BoundConditionalGotoStatement statement)
        {
            byte[] keyWordBytes = ToByte(KeywordCollection.ConditionalGotoStatement, 8);
            writer.WriteBytes(keyWordBytes);
            int[] address = addressManager.get(statement.boundLabel.name);
            writer.WriteBytes(ToByte(address, 8));

            WriteExpression(statement.condition);
        }

        protected override void WriteGotoStatement(BoundGotoStatement statement)
        {
            byte[] keyWordBytes = ToByte(KeywordCollection.GotoStatement, 8);
            writer.WriteBytes(keyWordBytes);
            int[] address = addressManager.get(statement.BoundLabel.name);
            writer.WriteBytes(ToByte(address, 8));
        }

        protected override void WriteReturnStatement(BoundReturnStatement statement)
        {
            byte[] keyWordBytes = ToByte(KeywordCollection.Return, 8);
            writer.WriteBytes(keyWordBytes);

            WriteExpression(statement.expression);
        }

        protected override void WriteVariableDeclarationStatement(BoundVariableDeclarationStatement statement)
        {
            if (statement.variable.isReadOnly)
            {
                byte[] keyWordBytes = ToByte(KeywordCollection.Const, 8);
                writer.WriteBytes(keyWordBytes);
            }
            else
            {
                byte[] keyWordBytes = ToByte(KeywordCollection.Let, 8);
                writer.WriteBytes(keyWordBytes);
            }

            int[] address = addressManager.get(statement.variable.name);
            writer.WriteBytes(ToByte(address, 8));

            WriteExpression(statement.initializer);
        }

        protected override void WriteExpressionStatement(BoundExpressionStatement statement)
        {
            WriteExpression(statement.expression);
        }

        protected override void WriteBlockStatement(BoundBlockStatement statement)
        {
            foreach (BoundStatement boundStatement in statement.statements)
            {
                WriteStatement(boundStatement);
                writer.WriteBytes(ToByte(KeywordCollection.CloseCommand, 8));
            }
        }

        protected override void WriteUnaryExpression(BoundUnaryExpression expression)
        {
            KeywordCollection keywordCollection;

            switch (expression.unaryOperator.operatorType)
            {
                case BoundUnaryOperatorType.Identity:
                    keywordCollection = KeywordCollection.Identity;
                    break;
                case BoundUnaryOperatorType.Negation:
                    keywordCollection = KeywordCollection.Negation;
                    break;
                case BoundUnaryOperatorType.LogicalNegation:
                    keywordCollection = KeywordCollection.LogicalNegation;
                    break;
                case BoundUnaryOperatorType.OnesComplement:
                    keywordCollection = KeywordCollection.OnesComplement;
                    break;
                default:
                    throw new Exception();
            }

            writer.WriteBytes(ToByte(keywordCollection, 8));
            WriteExpression(expression.right);
        }

        protected override void WriteBinaryExpression(BoundBinaryExpression expression)
        {
            KeywordCollection keywordCollection;

            switch (expression.binaryOperator.operatorType)
            {
                case BoundBinaryOperatorType.Addition:
                    keywordCollection = KeywordCollection.Addition;
                    break;
                case BoundBinaryOperatorType.Subtraction:
                    keywordCollection = KeywordCollection.Subtraction;
                    break;
                case BoundBinaryOperatorType.Multiplication:
                    keywordCollection = KeywordCollection.Multiplication;
                    break;
                case BoundBinaryOperatorType.Division:
                    keywordCollection = KeywordCollection.Division;
                    break;
                case BoundBinaryOperatorType.Modulo:
                    keywordCollection = KeywordCollection.Modulo;
                    break;
                case BoundBinaryOperatorType.Pow:
                    keywordCollection = KeywordCollection.Pow;
                    break;
                case BoundBinaryOperatorType.LogicalAnd:
                    keywordCollection = KeywordCollection.LogicalAnd;
                    break;
                case BoundBinaryOperatorType.LogicalOr:
                    keywordCollection = KeywordCollection.LogicalOr;
                    break;
                case BoundBinaryOperatorType.NotEquals:
                    keywordCollection = KeywordCollection.NotEquals;
                    break;
                case BoundBinaryOperatorType.Equals:
                    keywordCollection = KeywordCollection.Equals;
                    break;
                case BoundBinaryOperatorType.BitwiseAnd:
                    keywordCollection = KeywordCollection.BitwiseAnd;
                    break;
                case BoundBinaryOperatorType.BitwiseOr:
                    keywordCollection = KeywordCollection.BitwiseOr;
                    break;
                case BoundBinaryOperatorType.BitwiseXor:
                    keywordCollection = KeywordCollection.BitwiseXor;
                    break;
                case BoundBinaryOperatorType.BitwiseShiftLeft:
                    keywordCollection = KeywordCollection.BitwiseShiftLeft;
                    break;
                case BoundBinaryOperatorType.BitwiseShiftRight:
                    keywordCollection = KeywordCollection.BitwiseShiftRight;
                    break;
                case BoundBinaryOperatorType.Less:
                    keywordCollection = KeywordCollection.Less;
                    break;
                case BoundBinaryOperatorType.LessEquals:
                    keywordCollection = KeywordCollection.LessEquals;
                    break;
                case BoundBinaryOperatorType.Greater:
                    keywordCollection = KeywordCollection.Greater;
                    break;
                case BoundBinaryOperatorType.GreaterEquals:
                    keywordCollection = KeywordCollection.GreaterEquals;
                    break;
                default:
                    throw new Exception();
            }

            WriteExpression(expression.left);
            writer.WriteBytes(ToByte(keywordCollection, 8));
            WriteExpression(expression.right);
        }

        protected override void WriteAssignmentExpression(BoundAssignmentExpression expression)
        {
            int[] address = addressManager.get(expression.variableSymbol.name);
            writer.WriteBytes(ToByte(address, 8));
            WriteExpression(expression.expression);
        }

        protected override void WriteLiteralExpression(BoundLiteralExpression expression)
        {
            if (expression.type == TypeSymbol.Bool)
            {
                writer.WriteBytes(ToByte(KeywordCollection.LiteralBool, 8));
                int value = Convert.ToInt32(expression.value);

                writer.WriteBytes(ToByte(value, 8));
            }
            else if (expression.type == TypeSymbol.Int)
            {
                writer.WriteBytes(ToByte(KeywordCollection.LiteralInt, 8));
                writer.WriteBytes(ToByte((int)expression.value, 32));
            }
            else if (expression.type == TypeSymbol.String)
            {
                writer.WriteBytes(ToByte(KeywordCollection.LiteralString, 8));
                string value = (string)expression.value;
                int length = value.Length;
                int count8 = (length - length % 8) / 8;
                if (length % 8 != 0)
                {
                    count8++;
                }

                for (int i = 0; i < count8; i++)
                {
                    byte[] bytes = GenerateEmpyt(8);
                    for (int j = 0; j < 8; j++)
                    {
                        int index = (i * 8) + j;
                        if (index > length - 1)
                        {
                            break;
                        }

                        bytes[j] = Convert.ToByte(value[index]);
                    }


                    writer.WriteBytes(bytes);
                }
            }
        }

        protected override void WriteVariableExpression(BoundVariableExpression expression)
        {
            writer.WriteBytes(ToByte(addressManager.get(expression.variableSymbol.name), 8));
        }

        protected override void WriteCallExpression(BoundCallExpression expression)
        {
            /*
                call <name>
                <parameters>
             */

            writer.WriteBytes(ToByte(KeywordCollection.Call, 8));
            writer.WriteBytes(ToByte(addressManager.get(expression.function.name), 8));
            bool first = true;
            foreach (BoundExpression argument in expression.arguments)
            {
                if (!first)
                {
                    writer.WriteBytes(ToByte(KeywordCollection.SepSplit, 8));
                }
                else
                {
                    first = false;
                }

                WriteExpression(argument);
            }
        }

        protected override void WriteConversionExpression(BoundConversionExpression expression)
        {
            writer.WriteBytes(ToByte(KeywordCollection.Call, 8));
            writer.WriteBytes(ToByte(addressManager.get(expression.type.name), 8));

            WriteExpression(expression.expression);
        }
    }
}