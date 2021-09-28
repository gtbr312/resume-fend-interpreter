abstract class Expr
{

   static class Literal : Expr
    {
       public readonly Object value;

       public Literal (Object value)
       {
       this.value = value;
       }
    }


   static class Unary : Expr
    {
       public readonly Token operator;
       public readonly Expr right;

       public Unary (Token operator, Expr right)
       {
       this.operator = operator;
       this.right = right;
       }
    }


   static class Binary : Expr
    {
       public readonly Expr left;
       public readonly Token operator;
       public readonly Expr right;

       public Binary (Expr left, Token operator, Expr right)
       {
       this.left = left;
       this.operator = operator;
       this.right = right;
       }
    }


   static class Grouping : Expr
    {
       public readonly Expr expression;

       public Grouping (Expr expression)
       {
       this.expression = expression;
       }
    }

}
