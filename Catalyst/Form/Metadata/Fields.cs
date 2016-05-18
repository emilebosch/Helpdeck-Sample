using System.Collections.Generic;

namespace Catalyst
{
    public abstract class Field
    {
        public Field()
        {
            Errors = new List<string>();
        }
        public bool Required { get; set; }
        public string Hint { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public List<string> Errors { get; set; } 
    }
   
    public class LookupField : Field
    {
        public string LookupEntity { get; set; }
    }

    public class TextField : Field {}    
    public class NumericField : Field {}
    public class MemoField : Field {}
    public class BooleanField : Field {}
    public class DateField : Field {}
    public class Collection : Dictionary<string, string> {}

    public class CollectionField : Field
    {
        public Collection Collection { get; set; }
        public bool Multiple { get; set; }
        public CollectionField()
        {
            Collection = new Collection();
        }
    }

    public class LocationField : Field {}

    public class Location
    {
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
    }

    public class Address {}
}