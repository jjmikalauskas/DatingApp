using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    // [Authorize]
    [Route("api/users/{userid}/photos")]
    [ApiController]    

    public class PhotosController : ControllerBase
    {
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repository;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

    //CloudName": "five-star-software",
    //"ApiKey": "976989886787795",
    //"ApiSecret": "Kop52kVY7rG_6nupmAZJ7cYLkBc"

            // This is not reading then out of the config correctly! 
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName, 
                _cloudinaryConfig.Value.ApiKey, 
                _cloudinaryConfig.Value.ApiSecret
            );

            acc.Cloud = "five-star-software";
            acc.ApiKey = "976989886787795"; 
            acc.ApiSecret = "Kop52kVY7rG_6nupmAZJ7cYLkBc";

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);            
        }


        [HttpPost]
        // [EnableCors("AllowAnyHeaders")]
        // [RequestFormSizeLimit(valueCountLimit: 5000)]
        public async Task<IActionResult> AddPhotoToUser(int userId,
                                                        [FromForm] PhotoForCreationDto photoForCreationDto) // photoForCreationDto)
        {
            try {
                var currentUser = User.FindFirst(ClaimTypes.Name);
                if (currentUser!=null && userId != int.Parse(currentUser.Value))
                    return Unauthorized();
            }
            catch (Exception ex) {
            }
            
            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file?.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams() { 
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true; 

            userFromRepo.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                 var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                // Need to add userId = userId...for 3.0
                CreatedAtRouteResult carr = CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
                return carr;
            }

            return BadRequest("Could not add photo");
        }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            try {
                var currentUser = User.FindFirst(ClaimTypes.Name);
                if (currentUser!=null && userId != int.Parse(currentUser.Value))
                    return Unauthorized();
            }
            catch (Exception ex) {
            }

            var userFromRepo = await _repo.GetUser(userId);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id); 

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _repo.GetMainPhoto(userId);
            if (currentMainPhoto==null)
                return BadRequest("No main photo found!");

            currentMainPhoto.IsMain = false; 
            photoFromRepo.IsMain = true; 

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }
    
    }
}