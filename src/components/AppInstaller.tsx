import React, { useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Download, CheckCircle2, Globe, Gamepad2, MessageSquare, Wrench } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';
import { cn } from '../lib/utils';

type AppCategory = 'Browsers' | 'Gaming' | 'Communication' | 'Utilities';

interface AppItem {
  id: string;
  name: string;
  category: AppCategory;
  description: string;
  icon: React.ElementType;
}

const APPS: AppItem[] = [
  // Browsers
  { id: 'brave', name: 'Brave', category: 'Browsers', description: 'Privacy-focused browser', icon: Globe },
  { id: 'chrome', name: 'Google Chrome', category: 'Browsers', description: 'Fast and secure browser', icon: Globe },
  { id: 'firefox', name: 'Firefox', category: 'Browsers', description: 'Open-source browser', icon: Globe },
  
  // Gaming
  { id: 'steam', name: 'Steam', category: 'Gaming', description: 'PC gaming platform', icon: Gamepad2 },
  { id: 'epic', name: 'Epic Games', category: 'Gaming', description: 'Epic Games Store', icon: Gamepad2 },
  { id: 'ea', name: 'EA App', category: 'Gaming', description: 'Electronic Arts platform', icon: Gamepad2 },
  
  // Communication
  { id: 'discord', name: 'Discord', category: 'Communication', description: 'Chat for gamers', icon: MessageSquare },
  { id: 'whatsapp', name: 'WhatsApp', category: 'Communication', description: 'Desktop messaging', icon: MessageSquare },
  { id: 'telegram', name: 'Telegram', category: 'Communication', description: 'Fast messaging app', icon: MessageSquare },
  
  // Utilities
  { id: '7zip', name: '7-Zip', category: 'Utilities', description: 'File archiver', icon: Wrench },
  { id: 'vlc', name: 'VLC Media Player', category: 'Utilities', description: 'Media player', icon: Wrench },
  { id: 'spotify', name: 'Spotify', category: 'Utilities', description: 'Music streaming', icon: Wrench },
];

export function AppInstaller() {
  const { t } = useLanguage();
  const { registerAction } = useActionHistory();
  const [installing, setInstalling] = useState<Set<string>>(new Set());
  const [installed, setInstalled] = useState<Set<string>>(new Set());

  const categories = [
    { id: 'Browsers', label: t('catBrowsers'), icon: Globe },
    { id: 'Gaming', label: t('catGaming'), icon: Gamepad2 },
    { id: 'Communication', label: t('catComms'), icon: MessageSquare },
    { id: 'Utilities', label: t('catUtilities'), icon: Wrench },
  ];

  const handleInstall = (app: AppItem) => {
    if (installing.has(app.id) || installed.has(app.id)) return;

    setInstalling(prev => new Set(prev).add(app.id));

    // Wait for the backend connection/response, showing requested status later
    setTimeout(() => {
      setInstalling(prev => {
        const next = new Set(prev);
        next.delete(app.id);
        return next;
      });
      setInstalled(prev => new Set(prev).add(app.id));
      
      registerAction({
        type: 'custom_tweak',
        title: `winget install ${app.name}`
      });
    }, 2500);
  };

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 flex flex-col h-full">
      <div>
        <h2 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">{t('appsTab')}</h2>
        <p className="text-zinc-500 dark:text-zinc-400 mt-1">{t('appsDesc')}</p>
      </div>

      <div className="space-y-10">
        {categories.map(category => {
          const categoryApps = APPS.filter(a => a.category === category.id);
          
          return (
            <div key={category.id} className="space-y-4">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-zinc-100 dark:bg-zinc-900/50 text-zinc-600 dark:text-zinc-400 rounded-lg border border-zinc-200 dark:border-zinc-800">
                  <category.icon className="w-5 h-5" />
                </div>
                <h3 className="text-lg font-medium text-zinc-900 dark:text-zinc-100">{category.label}</h3>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {categoryApps.map(app => (
                  <div key={app.id} className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-5 shadow-sm flex items-center justify-between group">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-zinc-100 dark:bg-zinc-800 rounded-xl text-zinc-600 dark:text-zinc-400">
                        <app.icon className="w-6 h-6" />
                      </div>
                      <div>
                        <h4 className="font-semibold text-zinc-900 dark:text-zinc-100">{app.name}</h4>
                        <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">{app.description}</p>
                      </div>
                    </div>

                    <button
                      onClick={() => handleInstall(app)}
                      disabled={installing.has(app.id) || installed.has(app.id)}
                      className={cn(
                        "relative overflow-hidden w-28 h-9 flex items-center justify-center rounded-lg font-medium text-xs transition-all disabled:opacity-80 disabled:cursor-not-allowed",
                        installed.has(app.id)
                          ? "bg-emerald-50 dark:bg-emerald-500/10 text-emerald-600 dark:text-emerald-500 border border-emerald-200 dark:border-emerald-500/20"
                          : "bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 hover:bg-zinc-800 dark:hover:bg-zinc-200"
                      )}
                    >
                      <AnimatePresence mode="wait">
                        {installing.has(app.id) ? (
                          <motion.div key="installing" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2">
                            <div className="w-3.5 h-3.5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                          </motion.div>
                        ) : installed.has(app.id) ? (
                          <motion.div key="installed" initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex items-center gap-1.5">
                            <CheckCircle2 className="w-4 h-4" />
                            <span>{t('requested')}</span>
                          </motion.div>
                        ) : (
                          <motion.div key="idle" className="flex items-center gap-1.5 justify-center w-full h-full">
                            <Download className="w-3.5 h-3.5" />
                            <span>{t('install')}</span>
                          </motion.div>
                        )}
                      </AnimatePresence>
                    </button>
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
