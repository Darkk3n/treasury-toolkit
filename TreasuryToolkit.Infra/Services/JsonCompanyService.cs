using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Infra.Services
{
    public class JsonCompanyService : ICompanyService
    {
        private readonly List<CompanyModel> _companies;

        public JsonCompanyService()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\companies.json");

                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    _companies = JsonSerializer.Deserialize<List<CompanyModel>>(jsonContent) ?? [];
                }
                else
                {
                    _companies = [new() { Id = "ERR", Name = "companies.json no encontrado" }];
                }
            }
            catch
            {
                _companies = [new() { Id = "ERR", Name = "Error al cargar lista de empresas" }];
            }
        }

        public IReadOnlyList<CompanyModel> GetCompanyNames() => _companies;
    }
}
