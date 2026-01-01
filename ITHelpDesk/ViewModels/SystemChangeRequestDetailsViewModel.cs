namespace ITHelpDesk.ViewModels;

public class SystemChangeRequestDetailsViewModel
{
    public string RequesterName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ChangeDescription { get; set; } = string.Empty;
    public string ChangeReason { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string ChangePriority { get; set; } = string.Empty;
    public string ChangeImpact { get; set; } = string.Empty;
    public string AffectedAssets { get; set; } = string.Empty;
    public string ImplementationPlan { get; set; } = string.Empty;
    public string BackoutPlan { get; set; } = string.Empty;
    public string ImplementerName { get; set; } = string.Empty;
    public string ExecutionDate { get; set; } = string.Empty;
    public bool ManagerApproved { get; set; } = false;

    public static SystemChangeRequestDetailsViewModel Parse(string? description)
    {
        var result = new SystemChangeRequestDetailsViewModel();
        
        if (string.IsNullOrWhiteSpace(description))
            return result;

        var lines = description.Split('\n');
        var currentSection = string.Empty;
        var sectionContent = string.Empty;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (trimmedLine.StartsWith("ManagerApprovalStatus="))
            {
                result.ManagerApproved = trimmedLine.Equals("ManagerApprovalStatus=Approved", StringComparison.OrdinalIgnoreCase);
            }
            else if (trimmedLine.StartsWith("Requester:"))
            {
                result.RequesterName = trimmedLine.Substring("Requester:".Length).Trim();
            }
            else if (trimmedLine.StartsWith("Department:"))
            {
                result.Department = trimmedLine.Substring("Department:".Length).Trim();
            }
            else if (trimmedLine.StartsWith("Phone:"))
            {
                result.PhoneNumber = trimmedLine.Substring("Phone:".Length).Trim();
            }
            else if (trimmedLine.StartsWith("Change Description:"))
            {
                currentSection = "ChangeDescription";
                sectionContent = string.Empty;
            }
            else if (trimmedLine.StartsWith("Change Reason:"))
            {
                if (currentSection == "ChangeDescription")
                    result.ChangeDescription = sectionContent.Trim();
                currentSection = "ChangeReason";
                sectionContent = string.Empty;
            }
            else if (trimmedLine.Equals("Impact:", StringComparison.OrdinalIgnoreCase))
            {
                if (currentSection == "ChangeReason")
                    result.ChangeReason = sectionContent.Trim();
                currentSection = "Impact";
                sectionContent = string.Empty;
            }
            else if (currentSection == "Impact" && trimmedLine.Contains("="))
            {
                // Parse impact data line: Type=1, Priority=1, Impact=1
                var impactParts = trimmedLine.Split(',');
                foreach (var part in impactParts)
                {
                    var trimmedPart = part.Trim();
                    if (trimmedPart.Contains("="))
                    {
                        var keyValue = trimmedPart.Split('=', 2);
                        if (keyValue.Length == 2)
                        {
                            var key = keyValue[0].Trim();
                            var value = keyValue[1].Trim();
                            
                            if (key.Equals("Type", StringComparison.OrdinalIgnoreCase))
                                result.ChangeType = GetImpactLabel(value);
                            else if (key.Equals("Priority", StringComparison.OrdinalIgnoreCase))
                                result.ChangePriority = GetPriorityLabel(value);
                            else if (key.Equals("Impact", StringComparison.OrdinalIgnoreCase))
                                result.ChangeImpact = GetImpactLevelLabel(value);
                        }
                    }
                }
                currentSection = string.Empty;
            }
            else if (trimmedLine.StartsWith("Affected Assets:"))
            {
                currentSection = "AffectedAssets";
                sectionContent = string.Empty;
            }
            else if (trimmedLine.StartsWith("Implementation Plan:"))
            {
                if (currentSection == "AffectedAssets")
                    result.AffectedAssets = sectionContent.Trim();
                currentSection = "ImplementationPlan";
                sectionContent = string.Empty;
            }
            else if (trimmedLine.StartsWith("Backout Plan:"))
            {
                if (currentSection == "ImplementationPlan")
                    result.ImplementationPlan = sectionContent.Trim();
                currentSection = "BackoutPlan";
                sectionContent = string.Empty;
            }
            else if (trimmedLine.StartsWith("Implementer:"))
            {
                if (currentSection == "BackoutPlan")
                    result.BackoutPlan = sectionContent.Trim();
                result.ImplementerName = trimmedLine.Substring("Implementer:".Length).Trim();
                currentSection = string.Empty;
            }
            else if (trimmedLine.StartsWith("Execution Date:"))
            {
                result.ExecutionDate = trimmedLine.Substring("Execution Date:".Length).Trim();
            }
            else if (!string.IsNullOrWhiteSpace(currentSection))
            {
                sectionContent += (sectionContent.Length > 0 ? "\n" : "") + trimmedLine;
            }
        }

        // Capture last section
        if (currentSection == "BackoutPlan")
            result.BackoutPlan = sectionContent.Trim();

        return result;
    }

    private static string GetImpactLabel(string value)
    {
        return value switch
        {
            "1" => "تغيير علا التطبيقات",
            "2" => "تغيير على المكونات الماديه",
            "3" => "تغيير شبكي",
            "4" => "تغيير على انظمة التشغيل",
            _ => value
        };
    }

    private static string GetPriorityLabel(string value)
    {
        return value switch
        {
            "1" => "طارئ",
            "2" => "عالي",
            "3" => "متوسط",
            "4" => "منخفض",
            "5" => "تغيير على قواعد البيانات",
            "6" => "تغيير في الاجراءات",
            "7" => "تغيير في امن المعلومات",
            "8" => "تغييرات اخرى",
            _ => value
        };
    }

    private static string GetImpactLevelLabel(string value)
    {
        return value switch
        {
            "1" => "منخفض",
            "2" => "متوسط",
            "3" => "عالي",
            _ => value
        };
    }
}
