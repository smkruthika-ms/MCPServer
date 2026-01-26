using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.Resources;

/// <summary>
/// MCP Resource type for Create Opportunity widget
/// Resources in MCP are used to provide widget HTML that clients can render
/// Based on TypeScript example: ui://widget/create_opportunity_form.html
/// </summary>
[McpServerResourceType]
public class CreateOpportunityResource
{
    private const string CreateOpportunityFormHtml = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Create New Opportunity</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #0078d4 0%, #005a9e 100%);
            color: white;
            padding: 24px;
            text-align: center;
        }
        .header h1 { font-size: 24px; margin-bottom: 8px; }
        .header p { opacity: 0.9; font-size: 14px; }
        .form-body { padding: 24px; }
        .form-group { margin-bottom: 20px; }
        .form-group label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #323130;
        }
        .form-group input, .form-group select, .form-group textarea {
            width: 100%;
            padding: 12px;
            border: 1px solid #d2d0ce;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.2s, box-shadow 0.2s;
        }
        .form-group input:focus, .form-group select:focus, .form-group textarea:focus {
            outline: none;
            border-color: #0078d4;
            box-shadow: 0 0 0 3px rgba(0,120,212,0.1);
        }
        .form-group textarea { resize: vertical; min-height: 100px; }
        .form-row { display: flex; gap: 16px; }
        .form-row .form-group { flex: 1; }
        .required::after { content: ' *'; color: #d13438; }
        .btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 12px 24px;
            border: none;
            border-radius: 6px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
        }
        .btn-primary {
            background: #0078d4;
            color: white;
        }
        .btn-primary:hover { background: #106ebe; }
        .btn-secondary {
            background: #f3f2f1;
            color: #323130;
        }
        .btn-secondary:hover { background: #e1dfdd; }
        .form-actions {
            display: flex;
            gap: 12px;
            justify-content: flex-end;
            padding-top: 16px;
            border-top: 1px solid #edebe9;
        }
        .loading { display: none; }
        .loading.active { display: flex; align-items: center; gap: 8px; }
        .spinner {
            width: 16px;
            height: 16px;
            border: 2px solid #f3f2f1;
            border-top-color: #0078d4;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
        }
        @keyframes spin { to { transform: rotate(360deg); } }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎯 Create New Opportunity</h1>
            <p>Fill in the details below to create a new sales opportunity</p>
        </div>
        <form id=""opportunityForm"" class=""form-body"">
            <div class=""form-group"">
                <label for=""name"" class=""required"">Opportunity Name</label>
                <input type=""text"" id=""name"" name=""name"" placeholder=""Enter opportunity name"" required>
            </div>
            
            <div class=""form-row"">
                <div class=""form-group"">
                    <label for=""accountName"" class=""required"">Account Name</label>
                    <input type=""text"" id=""accountName"" name=""accountName"" placeholder=""Customer account name"" required>
                </div>
                <div class=""form-group"">
                    <label for=""estimatedValue"">Estimated Value ($)</label>
                    <input type=""number"" id=""estimatedValue"" name=""estimatedValue"" placeholder=""0.00"" min=""0"" step=""0.01"">
                </div>
            </div>
            
            <div class=""form-row"">
                <div class=""form-group"">
                    <label for=""solutionArea"">Solution Area</label>
                    <select id=""solutionArea"" name=""solutionArea"">
                        <option value="""">Select solution area</option>
                        <option value=""Azure"">Azure</option>
                        <option value=""Microsoft365"">Microsoft 365</option>
                        <option value=""Dynamics365"">Dynamics 365</option>
                        <option value=""PowerPlatform"">Power Platform</option>
                        <option value=""Security"">Security</option>
                        <option value=""DataAI"">Data & AI</option>
                    </select>
                </div>
                <div class=""form-group"">
                    <label for=""closeDate"">Expected Close Date</label>
                    <input type=""date"" id=""closeDate"" name=""closeDate"">
                </div>
            </div>
            
            <div class=""form-group"">
                <label for=""stage"">Sales Stage</label>
                <select id=""stage"" name=""stage"">
                    <option value=""qualify"">Qualify</option>
                    <option value=""develop"">Develop</option>
                    <option value=""propose"">Propose</option>
                    <option value=""close"">Close</option>
                </select>
            </div>
            
            <div class=""form-group"">
                <label for=""description"">Description</label>
                <textarea id=""description"" name=""description"" placeholder=""Describe the opportunity...""></textarea>
            </div>
            
            <div class=""form-actions"">
                <button type=""button"" class=""btn btn-secondary"" onclick=""resetForm()"">Reset</button>
                <button type=""submit"" class=""btn btn-primary"" id=""submitBtn"">
                    <span class=""btn-text"">Create Opportunity</span>
                    <span class=""loading"">
                        <span class=""spinner""></span>
                        Creating...
                    </span>
                </button>
            </div>
        </form>
    </div>
    
    <script>
        // Pre-populate form if MCP arguments are available
        document.addEventListener('DOMContentLoaded', function() {
            if (window.__mcpArguments) {
                const args = window.__mcpArguments;
                if (args.name) document.getElementById('name').value = args.name;
                if (args.accountName) document.getElementById('accountName').value = args.accountName;
                if (args.estimatedValue) document.getElementById('estimatedValue').value = args.estimatedValue;
                if (args.solutionArea) document.getElementById('solutionArea').value = args.solutionArea;
                if (args.closeDate) document.getElementById('closeDate').value = args.closeDate;
                if (args.stage) document.getElementById('stage').value = args.stage;
                if (args.description) document.getElementById('description').value = args.description;
            }
            
            // Also check OpenAI toolOutput if available
            if (window.openai?.toolOutput) {
                const output = window.openai.toolOutput;
                if (output.name) document.getElementById('name').value = output.name;
                if (output.accountName) document.getElementById('accountName').value = output.accountName;
            }
        });
        
        document.getElementById('opportunityForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const submitBtn = document.getElementById('submitBtn');
            const btnText = submitBtn.querySelector('.btn-text');
            const loading = submitBtn.querySelector('.loading');
            
            btnText.style.display = 'none';
            loading.classList.add('active');
            submitBtn.disabled = true;
            
            const formData = {
                name: document.getElementById('name').value,
                accountName: document.getElementById('accountName').value,
                estimatedValue: parseFloat(document.getElementById('estimatedValue').value) || 0,
                solutionArea: document.getElementById('solutionArea').value,
                closeDate: document.getElementById('closeDate').value,
                stage: document.getElementById('stage').value,
                description: document.getElementById('description').value
            };
            
            try {
                // Send to parent/agent via postMessage
                if (window.parent !== window) {
                    window.parent.postMessage({
                        type: 'opportunity-created',
                        data: formData
                    }, '*');
                }
                
                // If callback URL is available, also POST to it
                const serverUrl = window.__serverUrl || '';
                if (serverUrl) {
                    await fetch(serverUrl + '/widget-callback', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(formData)
                    });
                }
                
                alert('Opportunity created successfully!');
            } catch (error) {
                console.error('Error creating opportunity:', error);
                alert('Error creating opportunity: ' + error.message);
            } finally {
                btnText.style.display = 'inline';
                loading.classList.remove('active');
                submitBtn.disabled = false;
            }
        });
        
        function resetForm() {
            document.getElementById('opportunityForm').reset();
        }
    </script>
</body>
</html>";

    private const string OpportunityCreatedHtml = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Opportunity Created</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .container {
            max-width: 500px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            padding: 40px;
            text-align: center;
        }
        .success-icon {
            width: 80px;
            height: 80px;
            background: #107c10;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 24px;
            font-size: 40px;
            color: white;
        }
        h1 { color: #107c10; margin-bottom: 16px; }
        p { color: #605e5c; margin-bottom: 24px; }
        .details { 
            background: #f3f2f1; 
            border-radius: 8px; 
            padding: 16px; 
            text-align: left;
            margin-bottom: 24px;
        }
        .details dt { font-weight: 600; color: #323130; }
        .details dd { color: #605e5c; margin-bottom: 12px; }
        .btn {
            display: inline-block;
            padding: 12px 24px;
            background: #0078d4;
            color: white;
            border: none;
            border-radius: 6px;
            font-weight: 600;
            text-decoration: none;
            cursor: pointer;
        }
        .btn:hover { background: #106ebe; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""success-icon"">✓</div>
        <h1>Opportunity Created!</h1>
        <p>Your new opportunity has been successfully created.</p>
        <div class=""details"" id=""opportunityDetails"">
            <dl>
                <dt>Opportunity Name</dt>
                <dd id=""oppName"">Loading...</dd>
                <dt>Account</dt>
                <dd id=""oppAccount"">Loading...</dd>
                <dt>Estimated Value</dt>
                <dd id=""oppValue"">Loading...</dd>
            </dl>
        </div>
        <button class=""btn"" onclick=""openOpportunity()"">Open in Dynamics 365</button>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            const output = window.openai?.toolOutput || window.__mcpArguments || {};
            document.getElementById('oppName').textContent = output.name || output.opportunityName || 'N/A';
            document.getElementById('oppAccount').textContent = output.accountName || 'N/A';
            document.getElementById('oppValue').textContent = output.estimatedValue 
                ? '$' + parseFloat(output.estimatedValue).toLocaleString() 
                : 'N/A';
        });
        
        function openOpportunity() {
            const output = window.openai?.toolOutput || window.__mcpArguments || {};
            const opportunityId = output.opportunityId || output.id;
            if (opportunityId) {
                window.open('https://msxuat.crm.dynamics.com/main.aspx?appid=a6a3e35e-3ef9-eb11-94ef-000d3a5c5d0e&pagetype=entityrecord&etn=opportunity&id=' + opportunityId, '_blank');
            } else {
                alert('Opportunity ID not available');
            }
        }
    </script>
</body>
</html>";

    /// <summary>
    /// Resource for the Create Opportunity Form widget
    /// URI: ui://widget/create_opportunity_form.html
    /// </summary>
    [McpServerResource(
        UriTemplate = "ui://widget/create_opportunity_form.html",
        Name = "Create New Opportunity",
        MimeType = "text/html")]
    [Description("Create New Opportunity widget markup - Form for creating new sales opportunities")]
    public static string GetCreateOpportunityForm()
    {
        return CreateOpportunityFormHtml;
    }

    /// <summary>
    /// Resource for the Opportunity Created confirmation widget
    /// URI: ui://widget/opportunity_created.html
    /// </summary>
    [McpServerResource(
        UriTemplate = "ui://widget/opportunity_created.html",
        Name = "Opportunity Created",
        MimeType = "text/html")]
    [Description("Opportunity Created widget markup - Confirmation after opportunity creation")]
    public static string GetOpportunityCreatedWidget()
    {
        return OpportunityCreatedHtml;
    }
}
