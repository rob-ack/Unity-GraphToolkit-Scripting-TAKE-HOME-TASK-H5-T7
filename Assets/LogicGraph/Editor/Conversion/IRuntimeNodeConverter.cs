namespace LogicGraph.Editor.Conversion;

public interface IRuntimeNodeConverter
{
    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder);
}
