import React, { createContext, useContext, useState, ReactNode } from 'react';

export type ActionType = 'optimization' | 'ram_clean' | 'junk_clean' | 'network_reset';

export interface AppAction {
  id: string;
  type: ActionType;
  title: string;
  timestamp: Date;
  details?: string;
}

interface ActionHistoryContextType {
  lastAction: AppAction | null;
  registerAction: (action: Omit<AppAction, 'id' | 'timestamp'>) => void;
  undoLastAction: () => Promise<void>;
  clearAction: () => void;
  isUndoing: boolean;
}

const ActionHistoryContext = createContext<ActionHistoryContextType | null>(null);

export function ActionHistoryProvider({ children }: { children: ReactNode }) {
  const [lastAction, setLastAction] = useState<AppAction | null>(null);
  const [isUndoing, setIsUndoing] = useState(false);

  const registerAction = (action: Omit<AppAction, 'id' | 'timestamp'>) => {
    setLastAction({
      ...action,
      id: Math.random().toString(36).substring(2, 11),
      timestamp: new Date(),
    });
  };

  const undoLastAction = async () => {
    if (!lastAction) return;
    setIsUndoing(true);
    // Simulate network/OS call delay for reversing tweaks
    await new Promise(resolve => setTimeout(resolve, 2000));
    setLastAction(null);
    setIsUndoing(false);
  };
  
  const clearAction = () => {
    setLastAction(null);
  };

  return (
    <ActionHistoryContext.Provider value={{ lastAction, registerAction, undoLastAction, clearAction, isUndoing }}>
      {children}
    </ActionHistoryContext.Provider>
  );
}

export const useActionHistory = () => {
  const ctx = useContext(ActionHistoryContext);
  if (!ctx) throw new Error("useActionHistory must be used within an ActionHistoryProvider");
  return ctx;
};
