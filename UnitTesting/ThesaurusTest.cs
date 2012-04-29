using watch_assistant.Model.Dictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTesting
{
    
    
    /// <summary>
    ///Это класс теста для ThesaurusTest, в котором должны
    ///находиться все модульные тесты ThesaurusTest
    ///</summary>
    [TestClass()]
    public class ThesaurusTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region Other methods' tests
        ///// <summary>
        /////Тест для Конструктор Thesaurus
        /////</summary>
        //[TestMethod()]
        //public void ThesaurusConstructorTest()
        //{
        //    Thesaurus target = new Thesaurus();
        //    Assert.Inconclusive("TODO: реализуйте код для проверки целевого объекта");
        //}

        ///// <summary>
        /////Тест для Конструктор Thesaurus
        /////</summary>
        //[TestMethod()]
        //public void ThesaurusConstructorTest1()
        //{
        //    string filePath = string.Empty; // TODO: инициализация подходящего значения
        //    Thesaurus target = new Thesaurus(filePath);
        //    Assert.Inconclusive("TODO: реализуйте код для проверки целевого объекта");
        //}

        ///// <summary>
        /////Тест для AddDefinition
        /////</summary>
        //[TestMethod()]
        //public void AddDefinitionTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    IEnumerable<string> definition = null; // TODO: инициализация подходящего значения
        //    PermutationMethod mutual = new PermutationMethod(); // TODO: инициализация подходящего значения
        //    target.AddDefinition(key, definition, mutual);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для AddDefinition
        /////</summary>
        //[TestMethod()]
        //public void AddDefinitionTest1()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    string meaning = string.Empty; // TODO: инициализация подходящего значения
        //    PermutationMethod mutual = new PermutationMethod(); // TODO: инициализация подходящего значения
        //    target.AddDefinition(key, meaning, mutual);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для Deserialize
        /////</summary>
        //[TestMethod()]
        //public void DeserializeTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string filePath = string.Empty; // TODO: инициализация подходящего значения
        //    target.Deserialize(filePath);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для HasDefinitionFor
        /////</summary>
        //[TestMethod()]
        //public void HasDefinitionForTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.HasDefinitionFor(key);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для HasMeaningFor
        /////</summary>
        //[TestMethod()]
        //public void HasMeaningForTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    string meaning = string.Empty; // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.HasMeaningFor(key, meaning);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для RemoveDefinition
        /////</summary>
        //[TestMethod()]
        //public void RemoveDefinitionTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    PermutationMethod mutual = new PermutationMethod(); // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.RemoveDefinition(key, mutual);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для RemoveDefinition
        /////</summary>
        //[TestMethod()]
        //public void RemoveDefinitionTest1()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    string meaning = string.Empty; // TODO: инициализация подходящего значения
        //    PermutationMethod mutual = new PermutationMethod(); // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.RemoveDefinition(key, meaning, mutual);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для Serialize
        /////</summary>
        //[TestMethod()]
        //public void SerializeTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string filePath = string.Empty; // TODO: инициализация подходящего значения
        //    target.Serialize(filePath);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для SetDefinition
        /////</summary>
        //[TestMethod()]
        //public void SetDefinitionTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    IEnumerable<string> definition = null; // TODO: инициализация подходящего значения
        //    PermutationMethod mutual = new PermutationMethod(); // TODO: инициализация подходящего значения
        //    target.SetDefinition(key, definition, mutual);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для TryGetDefinition
        /////</summary>
        //[TestMethod()]
        //public void TryGetDefinitionTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    IEnumerable<string> definition = null; // TODO: инициализация подходящего значения
        //    IEnumerable<string> definitionExpected = null; // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.TryGetDefinition(key, out definition);
        //    Assert.AreEqual(definitionExpected, definition);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для Count
        /////</summary>
        //[TestMethod()]
        //public void CountTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    int actual;
        //    actual = target.Count;
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для Keys
        /////</summary>
        //[TestMethod()]
        //public void KeysTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    IEnumerable<string> actual;
        //    actual = target.Keys;
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для Name
        /////</summary>
        //[TestMethod()]
        //public void NameTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string expected = string.Empty; // TODO: инициализация подходящего значения
        //    string actual;
        //    target.Name = expected;
        //    actual = target.Name;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}
        #endregion (Other methods' tests)

        /// <summary>
        ///Тест для GetPhrasePermutations
        ///</summary>
        [TestMethod()]
        public void GetPhrasePermutationsTest()
        {
            Thesaurus target = new Thesaurus("..\\..\\..\\watch_assistant\\thesaurus.dic"); // TODO: инициализация подходящего значения
            string phrase = "красивый       <!#5>автомобиль. {15} ,ехал, \nпо         дороге, автомобильбудус"; // TODO: инициализация подходящего значения тестовая фраза, предназначенная для определения ошибок в алгоритме поиска интервалов подразделения.
            string[] expected = null; // TODO: инициализация подходящего значения
            string[] actual;
            actual = target.GetPhrasePermutations(phrase);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Проверьте правильность этого метода теста.");
        }
    }
}
