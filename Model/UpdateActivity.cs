using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyMongoNet.Model
{
    internal class UpdateActivity : UserActivity
    {
        public List<Variance> Diff { get; set; }

        public UpdateActivity() : this(null) { }

        public UpdateActivity(string Username) : base(Username)
        {
            ActivityType = ActivityType.Update;
        }

        public void SetDiff<T>(T oldObj, T newObj)
        {
            Diff = new List<Variance>();
            foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead))
            {
                object oldVal = prop.GetValue(oldObj);
                object newVal = prop.GetValue(newObj);
                bool areEqual = (newVal == oldVal);
                if (!areEqual && oldVal != null && newVal != null && !(newVal is string) && (newVal is IEnumerable))
                {
                    areEqual = true;
                    var enOld = ((IEnumerable)oldVal).GetEnumerator();
                    var enNew = ((IEnumerable)newVal).GetEnumerator();
                    while (true)
                    {
                        bool oldHasNext = enOld.MoveNext();
                        bool newHasNext = enNew.MoveNext();
                        if (!oldHasNext && !newHasNext)
                            break;
                        if (oldHasNext != newHasNext)
                        {
                            areEqual = false;
                            break;
                        }
                        if (oldHasNext && !enOld.Current.Equals(enNew.Current))
                        {
                            areEqual = false;
                            break;
                        }
                    }
                }
                if (!areEqual)
                    Diff.Add(new Variance { Prop = prop.Name, OldValue = oldVal, NewValue = newVal });
            }
        }

        public class Variance
        {
            public string Prop { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
        }
    }
}
