// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using JECS.MemoryFiles;
//
// namespace JECS.Tests
// {
//     // Tests that nodes can change types without errors
//     // Types of nodes in JECS: single-line, key children, list children, multi-line string
//     [TestClass]
//     public class NodeTypeChangesTests
//     {
//         const string SAVED_VALUE_KEY = "test key";
//
//         static readonly int VALUE_SINGLE_LINE = 123;
//         static readonly ComplexType VALUE_KEY_CHILDREN = new ComplexType(456, "hello world", true);
//         static readonly int[] VALUE_LIST_CHILDREN = new int[] { 123, 456, 789, 101112, 131415 };
//         static readonly string VALUE_MULTI_LINE_STRING = "hello\n\n\nworld\n\n\nlines!!!!\nwow";
//
//         [TestMethod]
//         public void NodeTypeChange_SingleLine_KeyChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_SingleLine_ListChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_SingleLine_MultiLineString()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//         }
//         
//         
//
//         [TestMethod]
//         public void NodeTypeChange_KeyChildren_SingleLine()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_KeyChildren_ListChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_KeyChildren_MultiLineString()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//         }
//         
//         
//
//         [TestMethod]
//         public void NodeTypeChange_ListChildren_SingleLine()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_ListChildren_KeyChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_ListChildren_MultiLineString()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//         }
//         
//         
//
//         [TestMethod]
//         public void NodeTypeChange_MultiLineString_SingleLine()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//             file.Set(SAVED_VALUE_KEY, VALUE_SINGLE_LINE);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_MultiLineString_KeyChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//             file.Set(SAVED_VALUE_KEY, VALUE_KEY_CHILDREN);
//         }
//
//         [TestMethod]
//         public void NodeTypeChange_MultiLineString_ListChildren()
//         {
//             var file = new MemoryDataFile();
//             
//             file.Set(SAVED_VALUE_KEY, VALUE_MULTI_LINE_STRING);
//             file.Set(SAVED_VALUE_KEY, VALUE_LIST_CHILDREN);
//         }
//     }
// }