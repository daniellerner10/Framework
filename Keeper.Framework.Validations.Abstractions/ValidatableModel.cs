using Keeper.Framework.Validations;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Keeper.Framework;

/// <summary>
/// Abstract class for validatable model.
/// </summary>
public abstract class ValidatableModel
{
    /// <summary>
    /// The list of issues.
    /// </summary>
    protected readonly List<ValidationIssue> Issues;

    internal bool ValidateGETMethods = false;

    /// <summary>
    /// Constructor for validatable models.
    /// </summary>
    /// <param name="validateGets">If true, validate gets.</param>
    protected ValidatableModel(bool validateGets = false)
    {
        ValidateGETMethods = validateGets;
        Issues = new List<ValidationIssue>();
    }

    /// <summary>
    /// Validate the model.
    /// </summary>
    /// <returns>A list of validation issues.</returns>
    public virtual IList<ValidationIssue> Validate()
    {
        //validate all data annotations
        var validationResults = new List<ValidationResult>();
        var vc = new ValidationContext(this, null, null);
        var isValid = Validator.TryValidateObject
            (this, vc, validationResults, true);

        if (!isValid)
        {
            Issues.AddRange(validationResults.Select(static vr => new ValidationIssue(vr.ErrorMessage!)));
        }

        //Issues.AddRange(this.ValidateStrings());

        // call validate recursively on members that inherit from ValidatableModel
        ValidateChildren();

        ValidateList();
        return Issues;
    }

    private void ValidateList()
    {
        var collectionsOfObjects = this.GetType()
            .GetProperties()
            .Where(static p => p.CustomAttributes.Any(static ca => ca.AttributeType == typeof(ListOfValidatbleModelAttribute))
                        && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)
                        && p.PropertyType.GetGenericArguments()
                            .Any(static ga => ga.IsSubclassOf(typeof(ValidatableModel))));
        foreach (var collection in collectionsOfObjects)
        {
            var collectionOfValidatableModel = collection.GetValue(this) as IEnumerable;
            if (collectionOfValidatableModel is not null)
                foreach (var obj in collectionOfValidatableModel)
                {
                    var v = obj as ValidatableModel;
                    Issues.AddRange(v?.Validate() ?? new List<ValidationIssue>());
                }
        }
    }

    private void ValidateChildren()
    {
        var children = GetType()
                      .GetProperties()
                      .Where(static p => p.PropertyType.IsSubclassOf(typeof(ValidatableModel)));

        foreach (var child in children)
        {
            var v = child.GetValue(this) as ValidatableModel;
            Issues.AddRange(v?.Validate() ?? new List<ValidationIssue>());
        }
    }

    /// <summary>
    /// Guard if is valid.
    /// </summary>
    public virtual void GuardIsValid()
    {
        var issues = Validate();
        if (issues.Count > 0)
            throw new KeeperValidationException(issues);
    }

    /// <summary>
    /// Adds a message to the issues.
    /// </summary>
    /// <param name="message">The message.</param>
    protected void Violate(string message)
    {
        Issues.Add(new ValidationIssue(message));
    }
}