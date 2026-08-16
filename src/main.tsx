import {StrictMode} from 'react';
import {createRoot} from 'react-dom/client';
import App from './App.tsx';
import { ErrorBoundary } from './components/ErrorBoundary';
import { LanguageProvider } from './contexts/LanguageContext.tsx';
import { ActionHistoryProvider } from './contexts/ActionHistoryContext.tsx';
import './index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <LanguageProvider>
      <ActionHistoryProvider>
        <ErrorBoundary><App /></ErrorBoundary>
      </ActionHistoryProvider>
    </LanguageProvider>
  </StrictMode>,
);
