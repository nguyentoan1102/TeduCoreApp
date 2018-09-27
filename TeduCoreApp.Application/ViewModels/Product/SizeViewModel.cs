﻿using System.ComponentModel.DataAnnotations;

namespace TeduCoreApp.Application.ViewModels.Product
{
    public class SizeViewModel
    {
        public int Id { get; set; }

        [StringLength(250)]
        public string Name
        {
            get; set;
        }
    }
}