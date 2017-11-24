using System.Collections.Generic;
using System.Linq;
using Impact.Core.Model;
using NUnit.Framework;

namespace Impact.Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestListCopy()
        {
            List<Week> orgList = new List<Week>
            {
                new Week {WorkHours = 10},
                new Week {WorkHours = 20},
                new Week {WorkHours = 30},
            };

            List<Week> copyList = orgList.ToList();

            copyList.RemoveAt(0);
            
            Assert.AreEqual(orgList.Count, 3);
            Assert.AreEqual(copyList.Count, 2);
        }
        
        [Test]
        public void TestOrgListEdit()
        {
            List<Week> orgList = new List<Week>
            {
                new Week {WorkHours = 10},
                new Week {WorkHours = 20},
                new Week {WorkHours = 30},
            };

            List<Week> copyList = orgList.ToList();

            var first = copyList.First();
            first.WorkHours += 5;
            
            // Side effect of reference
            Assert.AreEqual(15, orgList.First().WorkHours);
        }
        
        [Test]
        public void TestCopyListEdit()
        {
            List<Week> orgList = new List<Week>
            {
                new Week {WorkHours = 10},
                new Week {WorkHours = 20},
                new Week {WorkHours = 30},
            };

            List<Week> copyList = orgList.ToList();

            var first = copyList.First();
            first.WorkHours += 5;
            
            // Wanted effect
            Assert.AreEqual(15, copyList.First().WorkHours);
        }
        
        [Test]
        public void TestDeepCopyListEdit()
        {
            List<Week> orgList = new List<Week>
            {
                new Week {WorkHours = 10},
                new Week {WorkHours = 20},
                new Week {WorkHours = 30},
            };

            List<Week> copyList = orgList.ConvertAll(w => w.Clone());

            var first = copyList.First();
            first.WorkHours += 5;
            
            // No more side effects
            Assert.AreEqual(10, orgList.First().WorkHours);
            Assert.AreEqual(15, copyList.First().WorkHours);
        }
    }
}