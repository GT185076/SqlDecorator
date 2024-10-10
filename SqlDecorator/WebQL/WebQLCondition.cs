namespace SQLDecorator.WebQL
{
    public class WebQLCondition
    {
        public BooleanOperator MainOperator { get; set; }
        public string FirstOperand { get; set; }
        public ComparisonOperator ComparisonOperator { get; set; }
        public string SecondOperand { get; set; }
        public string ThirdOperand { get; set; }
        public WebQLCondition NextCondition { get; set; }
    }
}