using RestSharp;
using System.Net;
using System.Text.Json;

namespace RestSharpAPITests
{
    public class RestSharpAPI_Tests
    {
        private RestClient client;
        private const string baseUrl = "https://taskboardjs.dimomitev.repl.co/api";

        [SetUp]
        public void Setup()
        {
            this.client = new RestClient(baseUrl);
        }

        [Test]
        public void Test_GetDoneTasks_CheckTitle()
        {
            // Arrange
            var request = new RestRequest("tasks/board/done", Method.Get);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var tasks = JsonSerializer.Deserialize<List<Task>>(response.Content);
            Assert.That(tasks[0].title, Is.EqualTo("Project skeleton"));
        }

        [Test]
        public void Test_SearchByKeyword_ValidResults()
        {
            // Arrange
            var request = new RestRequest("tasks/search/home", Method.Get);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var tasks = JsonSerializer.Deserialize<List<Task>>(response.Content);
            Assert.That(tasks[0].title, Is.EqualTo("Home page"));
        }

        [Test]
        public void Test_SearchByKeyword_InvalidResults()
        {
            // Arrange
            var request = new RestRequest("tasks/search/missing" + DateTime.Now.Ticks, Method.Get);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.EqualTo("[]"));
        }

        [Test]
        public void Test_TryToCreateTask_MissingTitle()
        {
            // Arrange
            var request = new RestRequest("tasks", Method.Post);
            var reqBody = new
            {
                description = "some description",
                board = "Open"
            };
            request.AddBody(reqBody);

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Is.EqualTo("{\"errMsg\":\"Title cannot be empty!\"}"));
        }

        [Test]
        public void Test_CreateTask_ValidBody()
        {
            // Arrange
            var request = new RestRequest("tasks", Method.Post);
            var reqBody = new
            {
                title = "RESTSharp: Some title" + DateTime.Now.Ticks,
                description = "some description",
                board = "Open"
            };
            request.AddBody(reqBody);

            // Act
            var response = this.client.Execute(request);
            var taskObject = JsonSerializer.Deserialize<taskObject>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(taskObject.msg, Is.EqualTo("Task added."));
            Assert.That(taskObject.task.id, Is.GreaterThan(0));
            Assert.That(taskObject.task.title, Is.EqualTo(reqBody.title));
            Assert.That(taskObject.task.description, Is.EqualTo(reqBody.description));
            Assert.That(taskObject.task.board.id, Is.GreaterThan(0));
            Assert.That(taskObject.task.board.name, Is.EqualTo("Open"));
            Assert.That(taskObject.task.dateCreated, Is.Not.Empty);
            Assert.That(taskObject.task.dateModified, Is.Not.Empty);

        }
    }
}