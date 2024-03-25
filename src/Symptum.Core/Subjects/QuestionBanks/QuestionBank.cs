using System.Collections.ObjectModel;
using Symptum.Core.Management.Resources;

namespace Symptum.Core.Subjects.QuestionBanks;

public class QuestionBank : NavigableResource
{
    public QuestionBank()
    { }

    #region Properties

    private SubjectList _subjectCode;

    public SubjectList SubjectCode
    {
        get => _subjectCode;
        set => SetProperty(ref _subjectCode, value);
    }

    private ObservableCollection<QuestionBankPaper>? papers;

    public ObservableCollection<QuestionBankPaper>? Papers
    {
        get => papers;
        set
        {
            UnobserveCollection(papers);
            SetProperty(ref papers, value);
            SetChildrenResources(papers);
        }
    }

    #endregion

    protected override void OnInitializeResource(IResource? parent)
    {
        SetChildrenResources(papers);
    }

    public override bool CanHandleChildResourceType(Type childResourceType)
    {
        return childResourceType == typeof(QuestionBankPaper);
    }

    public override bool CanAddChildResourceType(Type childResourceType)
    {
        return childResourceType == typeof(QuestionBankPaper);
    }

    protected override void OnAddChildResource(IResource childResource)
    {
        Papers ??= [];
        if (childResource is QuestionBankPaper paper)
            papers?.Add(paper);
    }

    protected override void OnRemoveChildResource(IResource childResource)
    {
        throw new NotImplementedException();
    }
}
