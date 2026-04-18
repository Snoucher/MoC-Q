using examp.Models;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace examp
{
    [TestFixture]
    public class Tests
    {
        //initialize variables
        private RestClient client;
        private static string MovieId;
        private const string BaseURL = "http://144.91.123.158:5000/api";
        private const string UserEmail = "tost@tost";
        private const string UserPassword = "123456";

        //Grab Jwt Token
        private string GrabJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseURL);
            var request = new RestRequest("/User/Authentication", Method.Post);
            var authenticatorRequest = new AuthenticateDTO
            {
                Email = email,
                Password = password
            };
            request.AddJsonBody(authenticatorRequest);

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                return token;
            }
            else
            {
                throw new InvalidOperationException($"{response.StatusCode}, {response.Content}");
            }

            tempClient.Dispose();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseURL)
            {
                Authenticator = new JwtAuthenticator(GrabJwtToken(UserEmail, UserPassword))
            };

            this.client = new RestClient(options);
        }

        [Test]
        [Order(1)]
        public void createMovie()
        {
            //arrage
            var MovieRequest = new MovieDTO
            {
                Title = "Test",
                Description = "Test"
            };

            //act
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(MovieRequest);
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserialized.Movie, Is.Not.Null);
            Assert.That(deserialized.Movie.Id, Is.Not.Empty);
            Assert.That(deserialized.Movie.Id, Is.Not.Null);
            Assert.That(deserialized.Msg, Is.EqualTo("Movie created successfully!"));

            MovieId = deserialized.Movie.Id;
        }

        [Test]
        [Order(2)]
        public void editMovie()
        {
            //arrange
            var MovieRequest = new MovieDTO
            {
                Title = "edited",
                Description = "edited"
            };
            var request = new RestRequest("/Movie/Edit/", Method.Put);
            request.AddQueryParameter("movieId", MovieId);
            request.AddJsonBody(MovieRequest);


            //act
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserialized.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test]
        [Order(3)]
        public void getAllMovies()
        {
            //act
            var request = new RestRequest("/Catalog/All", Method.Get);
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized, Is.Not.Empty);
        }

        [Test]
        [Order(4)]
        public void deleteMovie()
        {
            //arrange
            var request = new RestRequest("/Movie/Delete/", Method.Delete);
            request.AddQueryParameter("movieId", MovieId);

            //act                        
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserialized.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test]
        [Order(5)]
        public void createBadResponseMovie()
        {
            //arrage
            var MovieRequest = new MovieDTO
            {
                Title = string.Empty,
                Description = string.Empty
            };

            //act
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(MovieRequest);
            var response = this.client.Execute(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        [Order(6)]
        public void editBadRequest()
        {


            //arrange
            var MovieRequest = new MovieDTO
            {
                Title = "edited",
                Description = "edited"
            };
            var request = new RestRequest("/Movie/Edit/", Method.Put);
            request.AddQueryParameter("movieId", "aurwtqifypo8wqhpo8w");
            request.AddJsonBody(MovieRequest);


            //act
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deserialized.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test]
        [Order(7)]
        public void deleteBadRequest()
        {
            //arrange
            var request = new RestRequest("/Movie/Delete/", Method.Delete);
            request.AddQueryParameter("movieId", "iuahnuqreuyfh7qp98wh7");

            //act                        
            var response = this.client.Execute(request);
            var deserialized = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content); ;

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deserialized.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void tearDown()
        {
            this.client.Dispose();
        }
    }
}