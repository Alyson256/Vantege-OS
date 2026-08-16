import React, { useState } from 'react';
import { Settings2, Play, Check, Save, History, XCircle, Clock, RotateCcw, ShieldCheck, Download, Info } from 'lucide-react';
import { TweakCategory } from '../types';
import { TWEAKS_DB } from '../data/tweaks.data';
import { cn } from '../lib/utils';
import { motion, AnimatePresence } from 'motion/react';
import { ConfirmationModal } from './ConfirmationModal';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';

interface LogEntry {
  id: string;
  tweakId: string;
  title: string;
  timestamp: Date;
  status: 'success' | 'error';
}

interface CustomTweaksProps {
  selectedTweaks: Set<string>;
  setSelectedTweaks: React.Dispatch<React.SetStateAction<Set<string>>>;
}

export function CustomTweaks({ selectedTweaks, setSelectedTweaks }: CustomTweaksProps) {
  const { registerAction } = useActionHistory();
  const [activeCategory, setActiveCategory] = useState<TweakCategory>('Performance');
  const [applying, setApplying] = useState(false);
  const [currentStepIndex, setCurrentStepIndex] = useState(0);
  const [success, setSuccess] = useState(false);
  const [saving, setSaving] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  
  const [undoing, setUndoing] = useState(false);
  const [undoSuccess, setUndoSuccess] = useState(false);
  const { lang, t } = useLanguage();

  const CATEGORIES: { id: TweakCategory; label: string }[] = [
    { id: 'Performance', label: t('catPerf') },
    { id: 'Privacy', label: t('catPriv') },
    { id: 'Network', label: t('catNet') },
    { id: 'System', label: t('catSys') },
    { id: 'UI', label: t('catUi') },
  ];

  const toggleTweak = (id: string) => {
    const newSet = new Set(selectedTweaks);
    if (newSet.has(id)) newSet.delete(id);
    else newSet.add(id);
    setSelectedTweaks(newSet);
  };

  const selectAllInCategory = () => {
    const newSet = new Set(selectedTweaks);
    const categoryTweaks = TWEAKS_DB.filter(t => t.category === activeCategory);
    const allSelected = categoryTweaks.every(t => newSet.has(t.id));
    
    categoryTweaks.forEach(t => {
      if (allSelected) newSet.delete(t.id);
      else newSet.add(t.id);
    });
    setSelectedTweaks(newSet);
  };

  const handleApplyClick = () => {
    if (selectedTweaks.size === 0) return;
    setIsModalOpen(true);
  };

  const confirmApplySelected = async () => {
    setIsModalOpen(false);
    if (selectedTweaks.size === 0) return;
    
    setApplying(true);
    setCurrentStepIndex(0);
    
    const newLogs: LogEntry[] = [];
    


    const tweaksArray = Array.from(selectedTweaks);
    
    // Reverse the initial logs if we want them at the top, or just add them sequentially.
    // We can just prepend each new log.
    if (newLogs.length > 0) {
       setLogs(prev => [...newLogs, ...prev]);
    }
    
    for (let i = 0; i < tweaksArray.length; i++) {
      setCurrentStepIndex(i);
      const id = tweaksArray[i];
      const tweak = TWEAKS_DB.find(t => t.id === id);
      
      // Simulate registry change latency
      await new Promise(resolve => setTimeout(resolve, 500));
      
      const logEntry = {
        id: Math.random().toString(36).substring(2, 11),
        tweakId: id,
        title: tweak ? tweak.title[lang] : id,
        timestamp: new Date(),
        status: 'success' as any
      };
      
      setLogs(prev => [logEntry, ...prev]);
    }
    registerAction({
      type: 'optimization',
      title: `${tweaksArray.length} ${t('custom')}`
    });

    setApplying(false);
    setSuccess(true);
    setTimeout(() => {
      setSuccess(false);
      setCurrentStepIndex(0);
    }, 3000);
  };

  const [isSaveModalOpen, setIsSaveModalOpen] = useState(false);
  const [saveSlot, setSaveSlot] = useState<1 | 2 | 3>(1);
  const [saveName, setSaveName] = useState("");

  const handleSaveProfile = () => {
    if (selectedTweaks.size === 0) return;
    setIsSaveModalOpen(true);
  };

  const confirmSaveProfile = () => {
    if (selectedTweaks.size === 0) return;
    try { 
      localStorage.setItem(`customTweaks_${saveSlot}`, JSON.stringify(Array.from(selectedTweaks)));
      if (saveName.trim()) {
        localStorage.setItem(`customProfileName_${saveSlot}`, saveName.trim());
      } else {
        localStorage.removeItem(`customProfileName_${saveSlot}`);
      }
    } catch (e) { console.error(e); }
    setIsSaveModalOpen(false);
    setSaveName("");
    setSaving(true);
    setTimeout(() => setSaving(false), 2500);
  };

  const handleUndoAll = () => {
    setUndoing(true);
    setTimeout(() => {
      setUndoing(false);
      setUndoSuccess(true);
      setLogs(prev => [{
        id: Math.random().toString(36).substring(2, 11),
        tweakId: 'undo_all',
        title: t('undoAllBat') as string,
        timestamp: new Date(),
        status: 'success'
      }, ...prev]);
      setTimeout(() => setUndoSuccess(false), 3000);
    }, 2500);
  };

  const handleExportLogs = () => {
    if (logs.length === 0) return;
    const logContent = logs.map(log => 
      `[${log.timestamp.toLocaleString()}] ${log.status.toUpperCase()}: ${log.title}`
    ).join('\n');
    
    const blob = new Blob([logContent], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'vantage-os-logs.txt';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const filteredTweaks = TWEAKS_DB.filter(t => t.category === activeCategory);

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 flex flex-col">
      
      <div className="flex flex-row justify-between gap-6 border-b border-zinc-200 dark:border-zinc-800 pb-6">
        
        {/* Left Section: Title & Tabs */}
        <div className="flex flex-col justify-between gap-8">
          <div>
            <h2 className="text-3xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50">{t('customTitle')}</h2>
            <p className="text-zinc-500 dark:text-zinc-400 mt-2 max-w-lg">{t('customDesc')}</p>
          </div>
          
          <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-hide mx-0 px-0">
            {CATEGORIES.map(cat => (
              <button
                key={cat.id}
                onClick={() => setActiveCategory(cat.id)}
                className={cn(
                  "px-4 py-2 rounded-full text-sm font-medium transition-colors whitespace-nowrap",
                  activeCategory === cat.id 
                    ? "bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900" 
                    : "bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-700"
                )}
              >
                {cat.label}
              </button>
            ))}
          </div>
        </div>

        {/* Right Section: Actions */}
        <div className="flex flex-col items-end justify-between gap-4">
          
          <div className="flex flex-wrap items-center gap-3">


            <button
              onClick={handleUndoAll}
              disabled={undoing}
              className="bg-white/50 dark:bg-zinc-800/50 text-zinc-700 dark:text-zinc-300 border border-zinc-200 dark:border-zinc-700 hover:border-amber-500/50 dark:hover:border-amber-500/50 hover:text-amber-600 dark:hover:text-amber-400 px-4 py-2 rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 text-sm"
            >
              <AnimatePresence mode="wait">
                {undoing ? (
                  <motion.div key="undoing" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2 text-amber-500">
                    <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                    <span className="inline">{t('undoing')}</span>
                  </motion.div>
                ) : undoSuccess ? (
                  <motion.div key="undoSuccess" initial={{ scale: 0.8 }} animate={{ scale: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2 text-emerald-500">
                    <Check className="w-4 h-4" />
                    <span className="inline">OK</span>
                  </motion.div>
                ) : (
                  <motion.div key="undo" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2">
                    <RotateCcw className="w-4 h-4" />
                    <span className="inline">{t('undoAll')}</span>
                  </motion.div>
                )}
              </AnimatePresence>
            </button>

            <button
              onClick={handleSaveProfile}
              disabled={selectedTweaks.size === 0 || saving}
              className="bg-white/50 dark:bg-zinc-800/50 text-zinc-700 dark:text-zinc-300 border border-zinc-200 dark:border-zinc-700 hover:border-purple-500/50 dark:hover:border-purple-500/50 hover:text-purple-600 dark:hover:text-purple-400 px-4 py-2 rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 text-sm"
            >
              {saving ? (
                <motion.div initial={{ scale: 0.8 }} animate={{ scale: 1 }} className="flex items-center gap-2 text-emerald-500 dark:text-emerald-600">
                  <Check className="w-4 h-4" />
                  <span className="inline">{t('savedSuccess')}</span>
                </motion.div>
              ) : (
                <div className="flex items-center gap-2">
                  <Save className="w-4 h-4" />
                  <span className="inline">{t('saveProfile')}</span>
                </div>
              )}
            </button>
          </div>

          <button
            onClick={handleApplyClick}
            disabled={selectedTweaks.size === 0 || applying}
            className={cn(
              "relative overflow-hidden px-6 py-2.5 rounded-xl font-medium transition-all flex items-center justify-center gap-2 min-w-[240px]",
              applying 
                ? "bg-zinc-800 dark:bg-zinc-800 text-white cursor-not-allowed" 
                : "bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 hover:bg-zinc-800 dark:hover:bg-zinc-200 disabled:opacity-50 disabled:cursor-not-allowed"
            )}
          >
            {applying && (
              <motion.div
                className="absolute left-0 top-0 bottom-0 bg-blue-600 dark:bg-blue-500"
                initial={{ width: 0 }}
                animate={{ width: `${((currentStepIndex + 1) / selectedTweaks.size) * 100}%` }}
                transition={{ type: 'spring', bounce: 0, duration: 0.5 }}
              />
            )}
            <AnimatePresence mode="wait">
              {applying ? (
                <motion.div key="loading" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="relative z-10 flex items-center gap-2 text-white">
                  <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  <span className="font-mono text-sm">{currentStepIndex + 1} / {selectedTweaks.size}</span>
                  <span>{t('executing')}</span>
                </motion.div>
              ) : success ? (
                <motion.div key="success" initial={{ scale: 0.8 }} animate={{ scale: 1 }} exit={{ opacity: 0 }} className="relative z-10 flex items-center gap-2 text-emerald-500 dark:text-emerald-600">
                  <Check className="w-5 h-5" />
                  <span>{t('applied')}</span>
                </motion.div>
              ) : (
                <motion.div key="idle" className="relative z-10 flex items-center gap-2">
                  <Play className="w-4 h-4 fill-current" />
                  <span>{t('applySelected')} ({selectedTweaks.size})</span>
                </motion.div>
              )}
            </AnimatePresence>
          </button>
        </div>
      </div>

      <div className="flex flex-col gap-8">
        <div className="flex flex-col">
          <div className="flex justify-between items-center mb-4 px-1">
            <span className="text-sm font-medium text-zinc-500 dark:text-zinc-400">
              {filteredTweaks.length} {t('optsAvail')}
            </span>
            <button onClick={selectAllInCategory} className="text-sm font-medium text-blue-600 dark:text-blue-400 hover:underline">
              {t('toggleAll')}
            </button>
          </div>
          
          <div className="grid grid-cols-3 gap-4">
            {filteredTweaks.map(tweak => {
              const isSelected = selectedTweaks.has(tweak.id);
              return (
                <div 
                  key={tweak.id} 
                  onClick={() => toggleTweak(tweak.id)}
                  className="p-5 flex flex-col gap-4 bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl hover:border-blue-500/50 dark:hover:border-blue-500/50 cursor-pointer transition-all shadow-sm hover:shadow-md relative group"
                >
                  <div className="flex justify-between items-start">
                    <div className="w-10 h-10 rounded-xl bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-zinc-600 dark:text-zinc-400">
                      <Settings2 className="w-5 h-5" />
                    </div>
                    
                    {/* Toggle Switch */}
                    <div className={cn(
                      "w-11 h-6 rounded-full flex items-center p-1 transition-colors duration-200",
                      isSelected ? "bg-emerald-500" : "bg-zinc-300 dark:bg-zinc-700"
                    )}>
                      <motion.div 
                        initial={false}
                        animate={{ x: isSelected ? 20 : 0 }}
                        className="w-4 h-4 bg-white rounded-full shadow-sm"
                      />
                    </div>
                  </div>

                  <div className="flex-1">
                    <h4 className="text-base font-semibold text-zinc-900 dark:text-zinc-100 leading-tight mb-2 flex items-center gap-2">
                      {tweak.title[lang]}
                      <div 
                        className="relative group/tooltip flex items-center"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Info className="w-4 h-4 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 transition-colors" />
                        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-72 p-3 bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 text-xs rounded-xl shadow-xl opacity-0 group-hover/tooltip:opacity-100 transition-opacity pointer-events-none z-50">
                          <div className="font-mono text-[10px] opacity-80 mb-1">{lang === 'pt' ? 'Detalhes do Registro' : 'Registry Details'}</div>
                          <div className="whitespace-pre-wrap font-mono leading-relaxed">{tweak.registryDetails[lang]}</div>
                          <div className="absolute top-full left-1/2 -translate-x-1/2 border-4 border-transparent border-t-zinc-900 dark:border-t-zinc-100" />
                        </div>
                      </div>
                    </h4>
                    <p className="text-sm text-zinc-500 dark:text-zinc-400 leading-relaxed line-clamp-3">
                      {tweak.description[lang]}
                    </p>
                  </div>
                  
                  <div className="pt-3 mt-auto border-t border-zinc-100 dark:border-zinc-800/80">
                    <p className="text-[11px] font-medium text-zinc-400 dark:text-zinc-500 uppercase tracking-wider">
                      {lang === 'pt' ? 'Recomendado:' : 'Recommended:'} <span className="text-amber-600 dark:text-amber-500">{tweak.recommendedFor.join(', ')}</span>
                    </p>
                  </div>
                </div>
              );
            })}
            {filteredTweaks.length === 0 && (
              <div className="col-span-full p-12 text-center text-zinc-500 dark:text-zinc-400 bg-white/50 dark:bg-zinc-900/50 rounded-2xl border border-zinc-200 dark:border-zinc-800">
                {t('noOpt')}
              </div>
            )}
          </div>
        </div>

        {/* History Panel */}
        <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl flex flex-col overflow-hidden max-h-[350px]">
          <div className="p-4 border-b border-zinc-200 dark:border-zinc-800 bg-zinc-50/50 dark:bg-zinc-900/50 flex items-center justify-between sticky top-0 z-10">
            <div className="flex items-center gap-2">
              <History className="w-5 h-5 text-zinc-500" />
              <h3 className="font-semibold text-zinc-900 dark:text-zinc-100">{t('historyTitle')}</h3>
            </div>
            
            <button
              onClick={handleExportLogs}
              disabled={logs.length === 0}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 text-zinc-600 dark:text-zinc-400 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-700 hover:text-zinc-900 dark:hover:text-zinc-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title={t('exportLogs')}
            >
              <Download className="w-3.5 h-3.5" />
              <span className="inline">{t('exportLogs')}</span>
            </button>
          </div>
          <div className="flex-1 overflow-y-auto p-4">
            {logs.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-zinc-500 dark:text-zinc-400 space-y-2 py-8">
                <Clock className="w-8 h-8 opacity-20" />
                <p className="text-sm">{t('emptyHistory')}</p>
              </div>
            ) : (
              <div className="grid grid-cols-3 gap-3">
                {logs.map(log => (
                  <motion.div 
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    key={log.id} 
                    className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white/80 dark:bg-zinc-900/80 flex items-start gap-3 shadow-sm"
                  >
                    {log.status === 'success' ? (
                      <div className="mt-0.5 w-6 h-6 rounded-full bg-emerald-100 dark:bg-emerald-500/10 flex items-center justify-center flex-shrink-0">
                        <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                      </div>
                    ) : (
                      <div className="mt-0.5 w-6 h-6 rounded-full bg-rose-100 dark:bg-rose-500/10 flex items-center justify-center flex-shrink-0">
                        <XCircle className="w-3.5 h-3.5 text-rose-600 dark:text-rose-400" />
                      </div>
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-zinc-900 dark:text-zinc-100 line-clamp-2 leading-tight">
                        {log.title}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        <span className={cn("text-xs font-medium", log.status === 'success' ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-600 dark:text-rose-400')}>
                          {log.status === 'success' ? t('statusSuccess') : t('statusError')}
                        </span>
                        <span className="text-xs text-zinc-400 dark:text-zinc-500 border-l border-zinc-300 dark:border-zinc-700 pl-2">
                          {log.timestamp.toLocaleTimeString()}
                        </span>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      <ConfirmationModal
        isOpen={isModalOpen}
        title={t('modalCustomTitle')}
        description={`${t('modalCustomDescPre')} ${selectedTweaks.size} ${t('modalCustomDescPost')}`}
        onConfirm={confirmApplySelected}
        onCancel={() => setIsModalOpen(false)}
        confirmText={t('confirmExec')}
        cancelText={t('review')}
      />
    </div>
  );
}
