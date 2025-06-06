// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT

using Cesium.CodeGen.Contexts;
using Cesium.CodeGen.Extensions;
using Cesium.CodeGen.Ir.Expressions.BinaryOperators;
using Cesium.CodeGen.Ir.Expressions.Values;
using Cesium.CodeGen.Ir.Types;
using Cesium.Core;

namespace Cesium.CodeGen.Ir.Expressions;

public enum AssignmentOperator
{
    Assign, // =
    AddAndAssign, // +=
    SubtractAndAssign, // -=
    MultiplyAndAssign, // *=
    DivideAndAssign, // /=

    BitwiseLeftShiftAndAssign, // <<=
    BitwiseRightShiftAndAssign, // >>=
    BitwiseOrAndAssign, // |=
    BitwiseAndAndAssign, // &=
    BitwiseXorAndAssign, // ^=
}

internal sealed class AssignmentExpression : IExpression
{
    private readonly bool _doReturn;

    public IValueExpression Left { get; }
    public IExpression Right{ get; }
    public AssignmentOperator Operator { get; }

    public AssignmentExpression(Ast.AssignmentExpression expression, bool doReturn, IDeclarationScope scope)
    {
        Operator = GetOperatorKind(expression.Operator);
        Left = expression.Left.ToIntermediate(scope) as IValueExpression
               ?? throw new AssertException($"Not a value expression: {expression.Left}.");
        Right = expression.Right.ToIntermediate(scope);
        _doReturn = doReturn;
    }

    public AssignmentExpression(IValueExpression left, AssignmentOperator @operator, IExpression right, bool doReturn)
    {
        Left = left;
        Operator = @operator;
        Right = right;
        _doReturn = doReturn;
    }

    public IExpression Lower(IDeclarationScope scope)
    {
        var rightExpanded = Operator switch
        {
            AssignmentOperator.Assign => Right,
            AssignmentOperator.AddAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.Add, Right),
            AssignmentOperator.SubtractAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.Subtract, Right),
            AssignmentOperator.MultiplyAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.Multiply, Right),
            AssignmentOperator.DivideAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.Divide, Right),
            AssignmentOperator.BitwiseLeftShiftAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.BitwiseLeftShift, Right),
            AssignmentOperator.BitwiseRightShiftAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.BitwiseRightShift, Right),
            AssignmentOperator.BitwiseOrAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.BitwiseOr, Right),
            AssignmentOperator.BitwiseAndAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.BitwiseAnd, Right),
            AssignmentOperator.BitwiseXorAndAssign => new BinaryOperatorExpression(Left, BinaryOperator.BitwiseXor, Right),
            _ => throw new WipException(226, $"Assignment operator not supported, yet: {Operator}.")
        };

        IExpression left = Left.Lower(scope);
        IExpression right = rightExpanded.Lower(scope);
        IType leftType = left.GetExpressionType(scope);
        IType rightType = right.GetExpressionType(scope);
        if (CTypeSystem.IsConversionAvailable(rightType, leftType)
            && CTypeSystem.IsConversionRequired(rightType, leftType))
        {
            right = new TypeCastExpression(leftType, right);
        }

        var value = ((IValueExpression)left).Resolve(scope);
        if (value is not ILValue lvalue)
            throw new CompilationException($"Not an lvalue: {value}.");

        return new SetValueExpression(lvalue, right, _doReturn);
    }

    // `x = v` expression returns type of x (and v)
    // e.g `int x; int y; x = (y = 10);`
    public IType GetExpressionType(IDeclarationScope scope) => Left.Resolve(scope).GetValueType();

    public void EmitTo(IEmitScope scope)
    {
        throw new AssertException("Should be lowered");
    }

    private static AssignmentOperator GetOperatorKind(string @operator) => @operator switch
    {
        "=" => AssignmentOperator.Assign,
        "+=" => AssignmentOperator.AddAndAssign,
        "-=" => AssignmentOperator.SubtractAndAssign,
        "*=" => AssignmentOperator.MultiplyAndAssign,
        "/=" => AssignmentOperator.DivideAndAssign,
        "<<=" => AssignmentOperator.BitwiseLeftShiftAndAssign,
        ">>=" => AssignmentOperator.BitwiseRightShiftAndAssign,
        "|=" => AssignmentOperator.BitwiseOrAndAssign,
        "&=" => AssignmentOperator.BitwiseAndAndAssign,
        "^=" => AssignmentOperator.BitwiseXorAndAssign,
        _ => throw new AssertException($"Invalid assignment operator: {@operator}.")
    };
}
