import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';

// Type definitions for window.openai
interface OpenAIToolOutput {
  title?: string;
  data?: any;
  items?: any[];
  summary?: string;
  [key: string]: any;
}

interface OpenAIGlobal {
  toolInput: any;
  toolOutput: OpenAIToolOutput;
  toolResponseMetadata: any;
  widgetState: any;
  setWidgetState: (state: any) => void;
  callTool: (name: string, args: any) => Promise<any>;
  sendFollowUpMessage: (options: { prompt: string }) => void;
  requestDisplayMode: (mode: 'pip' | 'fullscreen') => void;
  openExternal: (options: { href: string }) => void;
  theme: 'light' | 'dark';
  displayMode: string;
  locale: string;
}

declare global {
  interface Window {
    openai: OpenAIGlobal;
  }
}

// Hook to access and subscribe to window.openai
function useOpenAI<T>(selector: (openai: OpenAIGlobal) => T): T {
  const [value, setValue] = useState<T>(() => selector(window.openai));

  useEffect(() => {
    const handler = () => setValue(selector(window.openai));
    window.addEventListener('openai-update', handler);
    return () => window.removeEventListener('openai-update', handler);
  }, [selector]);

  return value;
}

// Hook for widget state management
function useWidgetState<T>(initialState: T | (() => T)) {
  const [state, setState] = useState<T>(() => {
    const existing = window.openai?.widgetState;
    return existing || (typeof initialState === 'function' ? (initialState as () => T)() : initialState);
  });

  const setWidgetState = (newState: T | ((prev: T) => T)) => {
    const updated = typeof newState === 'function' ? (newState as (prev: T) => T)(state) : newState;
    setState(updated);
    window.openai?.setWidgetState(updated);
  };

  return [state, setWidgetState] as const;
}

interface WidgetState {
  selectedTab: string;
  expandedItems: Set<string>;
}

const SalesWidget: React.FC = () => {
  const theme = useOpenAI(openai => openai.theme);
  const toolOutput = useOpenAI(openai => openai.toolOutput);
  const metadata = useOpenAI(openai => openai.toolResponseMetadata);
  
  const [widgetState, setWidgetState] = useWidgetState<WidgetState>({
    selectedTab: 'overview',
    expandedItems: new Set()
  });

  const handleRefresh = async () => {
    try {
      const result = await window.openai.callTool('SummarizeAccountProfile', {
        userPrompt: 'Refresh account data',
        context: ''
      });
      console.log('Refreshed:', result);
    } catch (error) {
      console.error('Failed to refresh:', error);
    }
  };

  const handleTabChange = (tab: string) => {
    setWidgetState(prev => ({ ...prev, selectedTab: tab }));
  };

  const toggleExpand = (itemId: string) => {
    setWidgetState(prev => {
      const expandedItems = new Set(prev.expandedItems);
      if (expandedItems.has(itemId)) {
        expandedItems.delete(itemId);
      } else {
        expandedItems.add(itemId);
      }
      return { ...prev, expandedItems };
    });
  };

  const renderContent = () => {
    if (!toolOutput) {
      return <div className="empty-state">No data available</div>;
    }

    // Handle different data structures
    if (toolOutput.summary) {
      return (
        <div className="summary-section">
          <h3>Summary</h3>
          <p>{toolOutput.summary}</p>
        </div>
      );
    }

    if (toolOutput.items && Array.isArray(toolOutput.items)) {
      return (
        <div className="items-list">
          {toolOutput.items.map((item: any, index: number) => (
            <div 
              key={index} 
              className="item-card"
              onClick={() => toggleExpand(`item-${index}`)}
            >
              <div className="item-header">
                <h4>{item.title || item.name || `Item ${index + 1}`}</h4>
                <span className="expand-icon">
                  {widgetState.expandedItems.has(`item-${index}`) ? '−' : '+'}
                </span>
              </div>
              {widgetState.expandedItems.has(`item-${index}`) && (
                <div className="item-details">
                  <pre>{JSON.stringify(item, null, 2)}</pre>
                </div>
              )}
            </div>
          ))}
        </div>
      );
    }

    // Fallback: display raw data
    return (
      <div className="raw-data">
        <pre>{JSON.stringify(toolOutput, null, 2)}</pre>
      </div>
    );
  };

  return (
    <div className={`sales-widget theme-${theme}`}>
      <div className="widget-header">
        <h2>{toolOutput?.title || 'Sales Dashboard'}</h2>
        <button onClick={handleRefresh} className="refresh-button">
          ↻ Refresh
        </button>
      </div>

      <div className="widget-tabs">
        <button
          className={widgetState.selectedTab === 'overview' ? 'active' : ''}
          onClick={() => handleTabChange('overview')}
        >
          Overview
        </button>
        <button
          className={widgetState.selectedTab === 'details' ? 'active' : ''}
          onClick={() => handleTabChange('details')}
        >
          Details
        </button>
        <button
          className={widgetState.selectedTab === 'metadata' ? 'active' : ''}
          onClick={() => handleTabChange('metadata')}
        >
          Metadata
        </button>
      </div>

      <div className="widget-content">
        {widgetState.selectedTab === 'overview' && renderContent()}
        {widgetState.selectedTab === 'details' && (
          <div className="details-view">
            <h3>Detailed Information</h3>
            <pre>{JSON.stringify(toolOutput, null, 2)}</pre>
          </div>
        )}
        {widgetState.selectedTab === 'metadata' && (
          <div className="metadata-view">
            <h3>Response Metadata</h3>
            <pre>{JSON.stringify(metadata, null, 2)}</pre>
          </div>
        )}
      </div>

      <div className="widget-footer">
        <small>Last updated: {new Date().toLocaleTimeString()}</small>
      </div>
    </div>
  );
};

// Initialize the widget
const root = document.getElementById('widget-root');
if (root) {
  createRoot(root).render(<SalesWidget />);
}
