﻿using ApteConsultancy.Data;
using ApteConsultancy.Dto;
using ApteConsultancy.Model.Master;
using ApteConsultancy.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace ApteConsultancy.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthService(AppDbContext appDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;

        }

        public async Task<bool> AssignRole(string email, string rolenamr)
        {
            var user = _appDbContext.applicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(rolenamr).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(rolenamr)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, rolenamr);
                return true;
            }
            return false;

        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _appDbContext.applicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (user == null || isValid == false)
            {
                return new LoginResponseDto()
                {
                    
                    Token = ""

                };
            }
            //if user was found, generate jwt tokem
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);
   
            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.EmployeeName,
                PhoneNumber = user.PhoneNumber,
                Token = token
            };
            return loginResponseDto;


        }

        public async Task<string> Register(RegisterationRequestDto registerationRequestDto)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDto.Email,
                Email = registerationRequestDto.Email,
                NormalizedEmail = registerationRequestDto.Email.ToUpper(),
                EmployeeName = registerationRequestDto.Name,
                PhoneNumber = registerationRequestDto.PhoneNumber,
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDto.Password); //inbuilt func hashes, entries password
                if (result.Succeeded)
                {

                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }

            }
            catch (Exception ex)
            {


            }
            return "Error encounterrd";
        }
    }
}
