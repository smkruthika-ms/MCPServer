import { Resource } from '@modelcontextprotocol/sdk/types.js';

/**
 * Creates the widget resource definition for the MCP server
 */
export function createWidgetResource(): Resource {
  return {
    uri: 'ui://widget/sales-dashboard.html',
    name: 'Sales Dashboard Widget',
    description: 'Interactive sales dashboard with tabs and data visualization',
    mimeType: 'text/html+skybridge', // Required for ChatGPT widgets
    _meta: {
      'openai/widgetPrefersBorder': true,
      'openai/widgetDomain': 'https://chatgpt.com',
      'openai/widgetDescription': 'Interactive sales dashboard for viewing account data, news, and analytics',
      'openai/widgetCSP': {
        connect_domains: ['https://chatgpt.com'],
        resource_domains: ['https://*.oaistatic.com'],
      },
    },
  };
}
