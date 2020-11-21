﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RepoController : ControllerBase
    {
        private readonly RepoService _repoService;
        public RepoController()
        {
            _repoService = new RepoService();
        }

        [HttpPost()]
        public async Task<IActionResult> AddRepo(RequestAddRepoDto addRepoDto)
        {
            var response = await _repoService.CheckRepoExist(addRepoDto.url);
            if (response.IsSucess)
            {
                RepositoryModel model = new RepositoryModel()
                {
                    ID = 123,
                    Name = response.name,
                    Owner = response.owner.login,
                    Url = response.html_url,
                    //Project = new ProjectModel()
                };
                try
                {
                    _repoService.CreateRepo(model);
                    return Ok(new ResponseDto
                    {
                        success = true,
                        message = "新增成功"
                    });
                }
                catch (Exception e)
                {
                    return Ok(new ResponseDto
                    {
                        success = false,
                        message = "新增失敗:" + e.Message
                    });

                }
            }
            else
            {
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "新增失敗:" + response.message
                });

            }

        }
    }
}