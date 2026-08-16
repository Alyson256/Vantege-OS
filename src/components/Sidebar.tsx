import React from 'react';
import { LayoutDashboard, Cpu, Sliders, Activity, Settings, Github, Heart, Globe, Moon, Sun, Package } from 'lucide-react';
import { Logo } from './Logo';
import { cn } from '../lib/utils';
import { useLanguage } from '../contexts/LanguageContext';
import { UpdateButton } from './UpdateButton';

type View = 'dashboard' | 'specs' | 'profiles' | 'custom' | 'network' | 'apps';

interface SidebarProps {
  activeView: View;
  setActiveView: (view: View) => void;
  showTour: boolean;
  tourStep: string;
  toggleLanguage: () => void;
  toggleTheme: () => void;
  isDark: boolean;
}

export function Sidebar({ activeView, setActiveView, showTour, tourStep, toggleLanguage, toggleTheme, isDark }: SidebarProps) {
  const { t, lang } = useLanguage();

  const NAV_ITEMS = [
    { id: 'dashboard', label: t('dashboard'), icon: LayoutDashboard },
    { id: 'specs', label: t('systemSpecs'), icon: Cpu },
    { id: 'profiles', label: t('profiles'), icon: Sliders },
    { id: 'apps', label: t('appsTab'), icon: Package },
    { id: 'custom', label: t('tourCustomTitle'), icon: Settings },
    { id: 'network', label: t('networkTab'), icon: Activity }
  ];

  return (
    <aside className="w-64 border-r border-zinc-200 dark:border-zinc-800 bg-white/50 dark:bg-zinc-900/20 backdrop-blur-xl flex flex-col">
      <div className="h-20 flex items-center gap-3 px-6 border-b border-zinc-200 dark:border-zinc-800">
        <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-zinc-800 to-zinc-900 dark:from-black dark:to-zinc-900 border border-zinc-200 dark:border-zinc-800 flex items-center justify-center">
          <Logo className="w-4 h-4" />
        </div>
        <div className="flex flex-col justify-center">
          <h1 className="font-bold text-base leading-none tracking-tight text-zinc-900 dark:text-zinc-50">Vantage OS</h1>
          <p className="text-[9px] mt-0.5 uppercase font-bold tracking-widest text-zinc-500 dark:text-zinc-500">{t('coreEngine')}</p>
        </div>
      </div>

      <nav className="flex-1 p-4 space-y-1.5 overflow-y-auto">
        {NAV_ITEMS.map(item => (
          <button
            key={item.id}
            onClick={() => setActiveView(item.id as View)}
            className={cn(
              "w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium text-sm transition-all",
              activeView === item.id 
                ? "bg-zinc-900 text-white dark:bg-zinc-100 dark:text-zinc-900 shadow-md"
                : "text-zinc-500 hover:text-zinc-900 hover:bg-zinc-100/80 dark:hover:text-zinc-100 dark:hover:bg-zinc-800/50",
              showTour && tourStep === item.id && "z-[101] relative bg-white dark:bg-zinc-800 shadow-xl ring-2 ring-blue-500 text-zinc-900 dark:text-zinc-100",
              showTour && tourStep !== item.id && "opacity-30 pointer-events-none"
            )}
          >
            <item.icon className="w-5 h-5" />
            {item.label}
          </button>
        ))}
      </nav>

      <div className={cn("p-4 border-t border-zinc-200 dark:border-zinc-800 space-y-1.5 transition-opacity", showTour && "opacity-30")}>
        <UpdateButton />
        
        <a 
          href="https://github.com" 
          target="_blank" 
          rel="noreferrer"
          className="w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium text-sm text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800/50 transition-colors"
        >
          <Github className="w-5 h-5" />
          {t('githubProj')}
        </a>

        <a 
          href="https://ko-fi.com" 
          target="_blank" 
          rel="noreferrer"
          className="w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium text-sm text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-500/10 hover:bg-rose-100 dark:hover:bg-rose-500/20 transition-colors"
        >
          <Heart className="w-5 h-5" />
          {t('supportProj')}
        </a>

        <button 
          onClick={toggleLanguage}
          className="w-full flex items-center justify-between px-4 py-3 rounded-xl font-medium text-sm text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800/50 transition-colors"
        >
          <span className="flex items-center gap-3">
            <Globe className="w-5 h-5" />
            {t('language')}
          </span>
          <span className="text-xs font-semibold px-2 py-1 rounded-md bg-zinc-200 dark:bg-zinc-800 uppercase">
            {lang}
          </span>
        </button>

        <button 
          onClick={toggleTheme}
          className="w-full flex items-center justify-between px-4 py-3 rounded-xl font-medium text-sm text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800/50 transition-colors"
        >
          <span className="flex items-center gap-3">
            {isDark ? <Moon className="w-5 h-5" /> : <Sun className="w-5 h-5" />}
            {t('theme')}
          </span>
          <span className="text-xs font-semibold px-2 py-1 rounded-md bg-zinc-200 dark:bg-zinc-800">
            {isDark ? t('dark') : t('light')}
          </span>
        </button>
      </div>
    </aside>
  );
}
