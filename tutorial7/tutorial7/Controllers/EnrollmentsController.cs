using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using tutorial7.DTOs;
using tutorial7.Models;
using tutorial7.Services;

namespace tutorial7.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    //[Authorize] every end points under this controller will have authorization
    public class EnrollmentsController : ControllerBase
    {
        private string ConnString = "Data Source=db-mssql;Initial Catalog=s19358;Integrated Security=True;MultipleActiveResultSets=True";

        IStudentsDbService service;
        public IConfiguration Configuration { get; set; }
        //constructor injection

        public EnrollmentsController(IStudentsDbService ser, IConfiguration configuration)
        {
            service = ser;
            Configuration = configuration;
        }


        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(Student student)
        {
            Student s = service.EnrollStudent(student);

            return Ok(s);

        }

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudent(Enrollment enrollment)
        {

            Enrollment en = service.PromoteStudent(enrollment);
            return Ok(en);

        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {

            bool validation = service.validationCredential(request.Login,request.Password);


            if (validation == false)
            {
                return Unauthorized("incorrect username or password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Name,"name1" ),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student"),
                 new Claim(ClaimTypes.Role, "employee")

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            //signing token
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",  //who issues this token
                audience: "Students",  //who can access it 
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()   //can be used when actual token is expired
            });


        }



    }
}