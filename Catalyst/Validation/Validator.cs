using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalyst
{
    public class Validator
    {
        Action<Validator> validator;
        Dictionary<string, List<string>> _errors;
        public Dictionary<string, List<string>> Errors { get { return _errors; } }

        public Validator(Action<Validator> onValidate = null)
        {
            _errors = new Dictionary<string, List<string>>();
            validator = onValidate;
        }

        public bool IsValid()
        {
            _errors.Clear();
            validator(this);
            return !Errors.Any();
        }

        public void ErrorIf(bool condition, string name, string errormessage, string id = null)
        {
            if (condition)
            {
                if (!_errors.ContainsKey(name))
                    _errors.Add(name, new List<string>());
                _errors[name].Add(errormessage);
            }
        }
    }

    public interface IValidatable
    {
        bool IsValid();
        Dictionary<string, List<string>> Errors { get; }
    }

    public static class ValidationExtensions
    {
        public static bool Between(this int target, int start, int end)
        {
            return (target >= start && target <= end);
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }
    }

    public interface IModel
    {
        void UpdateRelated(Relation relation, SimpleHash hash);
    }

    public static class ModelExtensions 
    {
        public static void UpdateAttributes(this IModel target, SimpleHash hash)
        {
            hash.Update(target);

            var validatable = target as IValidatable;
            if (validatable != null)
            {
                validatable.IsValid();
            }

            foreach (var relation in RelationDescriber.GetRelationsFor(target))
            {
                var postedeEntities = hash[relation.Name.ToLower()];
                if (postedeEntities != null)
                {
                    target.UpdateRelated(relation, postedeEntities);                  
                }
            }
        }
    }
}
