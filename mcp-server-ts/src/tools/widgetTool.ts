import { Tool } from '@modelcontextprotocol/sdk/types.js';

/**
 * Widget tool definition - Chat with Sales Agent and display results in a widget
 */
export const widgetTool: Tool = {
  name: 'widget_sales_chat',
  description:
    'Chat with the Sales Agent Plugin API and display the results in an interactive widget. ' +
    'This tool accepts a planner key, user message, and optional authentication token, ' +
    'then returns structured data that can be visualized in the ChatGPT widget.',
  inputSchema: {
    type: 'object',
    properties: {
      plannerKey: {
        type: 'string',
        description:
          'The planner key to use for the chat session (e.g., "SalesAgentPlanner", "AccountPlanner")',
      },
      message: {
        type: 'string',
        description: 'The user message or prompt to send to the sales agent',
      },
      token: {
        type: 'string',
        description:
          'Optional: Bearer token for authentication. If not provided, the server will attempt to acquire one.',
      },
    },
    required: ['plannerKey', 'message'],
  },
};
