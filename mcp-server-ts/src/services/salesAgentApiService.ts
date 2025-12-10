import * as dotenv from 'dotenv';

dotenv.config();

interface ChatRequest {
  InvokeAllFunctions: boolean;
  InvokeAllSkills: boolean;
  filterURLs: boolean;
  tracePlan: boolean;
  RequestId: string;
  Value: string;
  Inputs: Array<{ Key: string; Value: string }>;
  ExpeditionId: string;
  UserPrompt: string;
  PlannerKey: string;
  Source: string;
}

/**
 * Service for interacting with the Sales Agent Plugin API
 */
export class SalesAgentApiService {
  private apiUrl: string;

  constructor() {
    this.apiUrl =
      process.env.SALES_AGENT_API_URL ||
      'https://salescopilotskeusuat.azurewebsites.net/api/Playground';
  }

  /**
   * Chat with the sales agent
   */
  async chatWithAgent(
    plannerKey: string,
    message: string,
    token: string
  ): Promise<any> {
    console.log(`📤 Chatting with agent - PlannerKey: ${plannerKey}, Token length: ${token?.length || 0}`);

    const requestBody: ChatRequest = {
      InvokeAllFunctions: true,
      InvokeAllSkills: false,
      filterURLs: false,
      tracePlan: false,
      RequestId: this.generateGuid(),
      Value: message,
      Inputs: [
        { Key: 'useOboTokenGeneration', Value: 'true' },
        { Key: 'Debug', Value: 'yes' },
        { Key: 'SendAdapativeCardFormat', Value: 'yes' },
      ],
      ExpeditionId: this.generateGuid(),
      UserPrompt: message,
      PlannerKey: plannerKey,
      Source: 'MCPServerTS',
    };

    try {
      // Get authentication token if needed
      let authToken = token;
      if (!authToken) {
        console.log('⚠️  No token provided, attempting to acquire token...');
        authToken = await this.getToken();
      }

      const response = await fetch(this.apiUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(requestBody),
      });

      if (!response.ok) {
        throw new Error(`API request failed: ${response.status} ${response.statusText}`);
      }

      const result = await response.json();
      console.log('✅ Received response from Sales Agent API');
      return result;
    } catch (error) {
      console.error('❌ Error calling Sales Agent API:', error);
      throw error;
    }
  }

  /**
   * Get authentication token (stub - implement based on your auth flow)
   */
  private async getToken(): Promise<string> {
    // TODO: Implement token acquisition using Azure AD, Managed Identity, or OBO flow
    // For now, return empty string
    console.warn('⚠️  Token acquisition not implemented');
    return '';
  }

  /**
   * Generate a GUID for request tracking
   */
  private generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }
}
