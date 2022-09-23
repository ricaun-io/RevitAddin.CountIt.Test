using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitAddin.UnitTest
{
    public class CountItDocumentTest : IDisposable
    {
        const string fileName = "CountIt2.rvt";

        private readonly Document document;

        public CountItDocumentTest(Application application)
        {
            Assert.IsNotNull(application);

            this.document = application.OpenDocumentFile(fileName);
        }


        [Test]
        public void CountWall()
        {
            var walls = Select<Wall>(document);
            Assert.AreEqual(4, walls.Count);
        }

        [Test]
        public void CountFloors()
        {
            var floors = Select<Floor>(document);
            Assert.AreEqual(1, floors.Count);
        }

        [Test]
        public void CountDoors()
        {
            var doors = Select<FamilyInstance>(document, BuiltInCategory.OST_Doors);
            Assert.AreEqual(3, doors.Count);
        }

        [Test]
        public void CountWindows()
        {
            var windows = Select<FamilyInstance>(document, BuiltInCategory.OST_Windows);
            Assert.AreEqual(9, windows.Count);
        }

        public void Dispose()
        {
            document.Close(false);
            document.Dispose();
        }

        #region Select
        public ICollection<T> Select<T>(Document document) where T : Element
        {
            var elements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(T))
                .OfType<T>()
                .ToList();

            return elements;
        }

        public ICollection<T> Select<T>(Document document, BuiltInCategory builtInCategory) where T : Element
        {
            var elements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(T))
                .OfCategory(builtInCategory)
                .OfType<T>()
                .ToList();

            return elements;
        }
        #endregion
    }
}
