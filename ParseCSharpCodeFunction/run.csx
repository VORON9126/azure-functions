using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    var codeRequest = JsonConvert.DeserializeObject<CodeRequest>(requestBody);

    var tree = CSharpSyntaxTree.ParseText(codeRequest.Code);
    var root = tree.GetCompilationUnitRoot();

    var elements = new List<UIElement>();

    // Поиск элементов CuiPanel
    var panels = root.DescendantNodes()
        .OfType<ObjectCreationExpressionSyntax>()
        .Where(o => o.Type.ToString() == "CuiPanel");

    foreach (var panel in panels)
    {
        var element = new UIElement
        {
            Type = "panel",
            Properties = ExtractProperties(panel)
        };
        elements.Add(element);
    }

    // Поиск элементов CuiLabel
    var labels = root.DescendantNodes()
        .OfType<ObjectCreationExpressionSyntax>()
        .Where(o => o.Type.ToString() == "CuiLabel");

    foreach (var label in labels)
    {
        var element = new UIElement
        {
            Type = "label",
            Properties = ExtractProperties(label)
        };
        elements.Add(element);
    }

    // Поиск элементов CuiButton
    var buttons = root.DescendantNodes()
        .OfType<ObjectCreationExpressionSyntax>()
        .Where(o => o.Type.ToString() == "CuiButton");

    foreach (var button in buttons)
    {
        var element = new UIElement
        {
            Type = "button",
            Properties = ExtractProperties(button)
        };
        elements.Add(element);
    }

    return new OkObjectResult(elements);
}

private static Dictionary<string, string> ExtractProperties(ObjectCreationExpressionSyntax obj)
{
    var properties = new Dictionary<string, string>();

    foreach (var initializer in obj.Initializer.Expressions.OfType<AssignmentExpressionSyntax>())
    {
        var name = initializer.Left.ToString();
        var value = initializer.Right.ToString();
        properties[name] = value.Trim('"');
    }

    return properties;
}

public class CodeRequest
{
    public string Code { get; set; }
}

public class UIElement
{
    public string Type { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}
