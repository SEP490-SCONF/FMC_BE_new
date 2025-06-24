﻿using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.Service;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IPaperRepository _paperRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper; 
        public PapersController(IAzureBlobStorageService azureBlobStorageService,
                                IPaperRepository paperRepository,
                                IConfiguration configuration,
                                IMapper mapper)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _paperRepository = paperRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        
        [HttpPost("upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] PaperUploadDto paperDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (paperDto.PdfFile == null || paperDto.PdfFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            if (Path.GetExtension(paperDto.PdfFile.FileName)?.ToLower() != ".pdf")
            {
                return BadRequest("Only PDF files are allowed.");
            }

            try
            {
                var paperContainerName = _configuration.GetValue<string>("BlobContainers:Papers");
                if (string.IsNullOrEmpty(paperContainerName))
                {
                    return StatusCode(500, "Blob storage container name is not configured.");
                }

                string fileUrl = await _azureBlobStorageService.UploadFileAsync(paperDto.PdfFile, paperContainerName);

                var paper = _mapper.Map<Paper>(paperDto);

                paper.FilePath = fileUrl; 
                paper.SubmitDate = DateTime.UtcNow; 
                paper.Status = "Submitted"; 
                paper.IsPublished = false; 



                await _paperRepository.AddPaperAsync(paper);

                return Ok(new { Message = "File uploaded and paper data saved successfully.", FileUrl = fileUrl, PaperId = paper.PaperId });
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("view-pdf/{paperId}")]
        public async Task<IActionResult> ViewPdf(int paperId)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null || string.IsNullOrEmpty(paper.FilePath))
            {
                return NotFound("Paper or PDF file not found.");
            }

            return Redirect(paper.FilePath);
        }

 
        [HttpDelete("delete-pdf/{paperId}")]
        public async Task<IActionResult> DeletePdf(int paperId)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null || string.IsNullOrEmpty(paper.FilePath))
            {
                return NotFound("Paper or PDF file not found.");
            }

            try
            {
                // Xóa file khỏi Azure Blob Storage
                bool isDeleted = await _azureBlobStorageService.DeleteFileAsync(paper.FilePath);

                if (isDeleted)
                {
                    // Cập nhật đường dẫn file trong database thành null
                    paper.FilePath = null;
                    await _paperRepository.UpdatePaperAsync(paper);
                    return Ok("PDF file deleted successfully.");
                }
                else
                {
                    // Có thể file không tồn tại trên storage hoặc có lỗi khi xóa
                    return StatusCode(500, "Failed to delete PDF file from storage. It might not exist or an error occurred.");
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
