using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LumStoreAPI.Application.DTOs.Responses
{
    public class PagingModel
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; }
        [Range(1, 500)]
        public int PageSize { get; set; }
    }
}
