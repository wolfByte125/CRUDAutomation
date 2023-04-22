using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CRUDAutomation
{
    internal class CRUDAutomationService
    {
        public void IServiceCreator(Content content)
        {
            HeaderComponents headerComponents = new HeaderComponents();
            MethodSignatures methodSignatures = new MethodSignatures();
            string projectName = Assembly.GetCallingAssembly().GetName().Name;
            string createOrRequest = content.WithStatus ? "Request" : "Create";
            string idType = content.StringBasedId ? "string" : "int";
            string camelCaseModelName = char.ToLower(content.ModelName[0]) + content.ModelName.Substring(1);
            string pluralModel = Pluralize(content.ModelName);
            string interfaceContent = "";

            #region HEADER COMPONENTS INITIALIZATION
            headerComponents.ProjectName = projectName;
            headerComponents.ModelsNamespace = $"{headerComponents.ProjectName}.Models;";
            headerComponents.DTOsNamespace = $"{headerComponents.ProjectName}.DTOs.{content.ModelName}DTOs;";
            headerComponents.CurrentNamespace = $"{headerComponents.ProjectName}.Services.{content.ModelName}Services";
            #endregion

            #region METHOD SIGNATURES INITIALIZATION
            methodSignatures.GetAllMethodSignature = $"Task<List<{content.ModelName}>> Get{pluralModel}()";
            methodSignatures.GetSingleMethodSignature = $"Task<{content.ModelName}> Get{content.ModelName}({idType} id)";
            methodSignatures.CreateRequestMethodSignature = $"Task<{content.ModelName}> {createOrRequest + content.ModelName}({createOrRequest + content.ModelName}DTO {camelCaseModelName}DTO)";
            methodSignatures.UpdateMethodSignature = $"Task<{content.ModelName}> Update{content.ModelName}(Update{content.ModelName}DTO {camelCaseModelName}DTO)";
            methodSignatures.DeleteMethodSignature = $"Task<{content.ModelName}> Delete{content.ModelName}({idType} id)";
            methodSignatures.CheckMethodSignature = $"Task<{content.ModelName}> Check{pluralModel}(List<{idType}> ids)";
            methodSignatures.ApproveMethodSignature = $"Task<{content.ModelName}> Approve{pluralModel}(List<{idType}> ids)";
            methodSignatures.DeclineMethodSignature = $"Task<{content.ModelName}> Decline{pluralModel}(List<{idType}> ids)";
            #endregion

            #region CODE CONSTRUCTION
            interfaceContent =
                    $"using {headerComponents.ModelsNamespace}\n" +
                    $"using {headerComponents.DTOsNamespace}\n\n" +
                    $"namespace {headerComponents.CurrentNamespace}\n" +
                    $"{{\n" +
                    $"\tpublic interface I{content.ModelName}Service\n" +
                    $"\t{{\n" +
                    $"\t\t// CRUD\n" +
                    $"\t\t{methodSignatures.GetAllMethodSignature};\n" +
                    $"\t\t{methodSignatures.GetSingleMethodSignature};\n" +
                    $"\t\t{methodSignatures.CreateRequestMethodSignature};\n" +
                    $"\t\t{methodSignatures.UpdateMethodSignature};\n" +
                    $"\t\t{methodSignatures.DeleteMethodSignature};\n";

            if (content.WithStatus)
            {
                interfaceContent += 
                    $"\t\t// STATUS UPDATE\n" +
                    $"\t\t{methodSignatures.CheckMethodSignature};\n" +
                    $"\t\t{methodSignatures.ApproveMethodSignature};\n" +
                    $"\t\t{methodSignatures.DeclineMethodSignature};\n" +
                    $"\t}}\n" +
                    $"}}";
            }
            else
            {
                interfaceContent += 
                    $"\t}}\n" +
                    $"}}";
            }
            #endregion

            #region FILE WRITER
            FileWriter(new FileDetail
            {
                FileName = $"I{content.ModelName}Service",
                Content = interfaceContent
            });
            #endregion

            ServiceCreator(headerComponents, content, methodSignatures, createOrRequest, pluralModel, camelCaseModelName);
        }
        public void ServiceCreator(
            HeaderComponents headerComponents, 
            Content content, 
            MethodSignatures methodSignatures, 
            string createOrRequest,
            string pluralModel,
            string camelCaseModelName)
        {
            string classContent = "";

            #region CODE CONSTRUCTION
            classContent =
                $"using Microsoft.EntityFrameworkCore;\n" +
                $"using AutoMapper;\n" +
                $"using {headerComponents.ProjectName}.Context\n" +
                $"using {headerComponents.ModelsNamespace}\n" +
                $"using {headerComponents.DTOsNamespace}\n\n" +
                $"namespace {headerComponents.CurrentNamespace}\n" +
                $"{{\n" +
                $"\tpublic class {content.ModelName}Service : I{content.ModelName}Service\n" +
                $"\t{{\n" +
                $"\t\tprivate readonly DataContext _context;\n" +
                $"\t\tprivate readonly IMapper _mapper;\n" +
                $"\t\tpublic {content.ModelName}Service(DataContext context, IMapper mapper)\n" +
                $"\t\t{{\n" +
                $"\t\t\t_context = context;\n" +
                $"\t\t\t_mapper = mapper;\n" +
                $"\t\t}}\n\n";

            #region METHOD CONSTRUCTION
            GetMethodsCreator(ref classContent, methodSignatures, pluralModel, content.ModelName, camelCaseModelName);
            // CREATE / REQUEST
            CreateRequestMethodCreator(ref classContent, methodSignatures, createOrRequest, pluralModel, content.ModelName, camelCaseModelName);
            // UPDATE
            UpdateMethodsCreator(ref classContent, methodSignatures, content.ModelName, camelCaseModelName);
            // DELETE
            DeleteMethodsCreator(ref classContent, methodSignatures, content.ModelName, camelCaseModelName);
            if (content.WithStatus)
            {
                // CHECK
                CheckMethodsCreator(ref classContent, methodSignatures, content.ModelName, camelCaseModelName);
                // APPROVE
                ApproveMethodsCreator(ref classContent, methodSignatures, content.ModelName, camelCaseModelName);
                // DECLINE
                DeclineMethodsCreator(ref classContent, methodSignatures, content.ModelName, camelCaseModelName);
            }
            #endregion

            // ADD CLOSING TAGS
            classContent += 
                $"\n\t}}\n" +
                $"}}";
            #endregion

            #region FILE WRITER
            FileWriter(new FileDetail
            {
                FileName = $"{content.ModelName}Service",
                Content = classContent
            });
            #endregion

        }

        // GETS CREATOR
        private void GetMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures, 
            string pluralModel,
            string modelName,
            string camelCaseModelName)
        {
            classContent += 
                $"\t\t// GET ALL\n" +
                $"\t\tpublic async {methodSignatures.GetAllMethodSignature}\n" +
                $"\t\t{{\n" +
                $"\t\t\tvar {char.ToLower(pluralModel[0]) + pluralModel.Substring(1)} = await _context.{pluralModel}\n" +
                $"\t\t\t\t.OrderByDescending(x => x.Id)\n" +
                $"\t\t\t\t.ToListAsync();\n" +
                $"\n" +
                $"\t\t\treturn {char.ToLower(pluralModel[0]) + pluralModel.Substring(1)};\n" +
                $"\t\t}}\n\n" +
                $"\t\t// GET SINGLE\n" +
                $"\t\tpublic async {methodSignatures.GetSingleMethodSignature}\n" +
                $"\t\t{{\n" +
                $"\t\t\tvar {camelCaseModelName} = await _context.{pluralModel}\n" +
                $"\t\t\t\t.Where(x => x.Id == id)\n" +
                $"\t\t\t\t.FirstOrDefaultAsync();\n" +
                $"\n" +
                $"\t\t\tif({camelCaseModelName} == null) throw new KeyNotFoundException(\"" + $"{SpaceOut(modelName)} Not Found.\"" + $");\n" +
                $"\n" +
                $"\t\t\treturn {camelCaseModelName};\n" +
                $"\t\t}}\n\n";
        }
        
        // CREATE CREATOR
        private void CreateRequestMethodCreator(
            ref string classContent, 
            MethodSignatures methodSignatures, 
            string createRequest,
            string pluralModel,
            string modelName,
            string camelCaseModelName)
        {
            classContent +=
                $"\t\t// CREATE || REQUEST\n" +
                $"\t\tpublic async {methodSignatures.CreateRequestMethodSignature}\n" +
                $"\t\t{{\n" +
                $"\t\t\tvar {camelCaseModelName} = _mapper.Map<{modelName}>({camelCaseModelName}DTO);\n" +
                $"\n" +
                $"\t\t\t_context.{pluralModel}.Add({camelCaseModelName});\n" +
                $"\t\t\tawait _context.SaveChangesAsync();\n" +
                $"\n" +
                $"\t\t\treturn {camelCaseModelName};\n" +
                $"\t\t}}\n\n";
        }

        // UPDATE CREATOR
        private void UpdateMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures,
            string modelName,
            string camelCaseModelName)
        {

        }

        // DELETE CREATOR
        private void DeleteMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures,
            string modelName,
            string camelCaseModelName)
        {

        
        }

        // CHECK CREATOR
        private void CheckMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures,
            string modelName,
            string camelCaseModelName)
        {

        }
        
        // APPROVE CREATOR
        private void ApproveMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures,
            string modelName,
            string camelCaseModelName)
        {

        }

        // DECLINE CREATOR
        private void DeclineMethodsCreator(
            ref string classContent, 
            MethodSignatures methodSignatures,
            string modelName,
            string camelCaseModelName)
        {

        }

        // GET THE PLURAL FORM OF A MODEL
        private string Pluralize(string single)
        {
            if (single.EndsWith("y"))
            {
                return (single.Remove(single.Length - 1, 1) + "ies");
            }
            else
            {
                return (single + "s");
            }
        }
        // WRITE CONTENTS TO FILE
        private void FileWriter(FileDetail fileDetail)
        {
            string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + '/';
            using (StreamWriter pen = new StreamWriter($"{projectDir}{fileDetail.FileName}.cs"))
            {
                    pen.WriteLine(fileDetail.Content);
            }
        }

        private string SpaceOut(string PascalCase)
        {
            var words = Regex.Matches(PascalCase, @"([A-Z][a-z]+)")
                .Cast<Match>()
                .Select(m => m.Value);

            return string.Join(" ", words);
        }
    }
}
