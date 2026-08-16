import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Dashboard } from './components/Dashboard';
import { Profiles } from './components/Profiles';
import { CustomTweaks } from './components/CustomTweaks';

import { NetworkAnalysis } from './components/NetworkAnalysis';
import { SystemSpecs } from './components/SystemSpecs';
import { Sidebar } from './components/Sidebar';
import { useLanguage } from './contexts/LanguageContext';

import { OnboardingTour } from './components/OnboardingTour';
import { GlobalUndoWidget } from './components/GlobalUndoWidget';
import { NpsPopup } from './components/NpsPopup';
import { InitialScanner } from './components/InitialScanner';
import { AppInstaller } from './components/AppInstaller';

type View = 'dashboard' | 'specs' | 'profiles' | 'custom' | 'network' | 'apps';

export default function App() {
  const [activeView, setActiveView] = useState<View>('dashboard');
  const [isDark, setIsDark] = useState(true);
  const [selectedTweaks, setSelectedTweaks] = useState<Set<string>>(new Set());
  const [showTour, setShowTour] = useState(false);
  const [isScanning, setIsScanning] = useState(true);
  const [tourStep, setTourStep] = useState<string>('welcome');
  const { lang, setLang } = useLanguage();

  useEffect(() => {
    let hasSeenTour = null; try { hasSeenTour = localStorage.getItem('hasSeenTour'); } catch(e){}
    if (!hasSeenTour) {
      setShowTour(true);
    }
  }, []);

  const handleCompleteTour = () => {
    try { localStorage.setItem('hasSeenTour', 'true'); } catch(e){}
    setShowTour(false);
  };

  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [isDark]);

  const toggleTheme = () => setIsDark(!isDark);
  const toggleLanguage = () => setLang(lang === 'pt' ? 'en' : 'pt');

  return (
    <>
      <AnimatePresence>
        {isScanning && <InitialScanner onComplete={() => setIsScanning(false)} />}
      </AnimatePresence>
      <div className="flex h-screen bg-zinc-50 dark:bg-black text-zinc-900 dark:text-zinc-100 font-sans selection:bg-blue-500/30 overflow-hidden selection:text-blue-900 dark:selection:text-blue-100">
      <Sidebar 
        activeView={activeView} 
        setActiveView={setActiveView} 
        showTour={showTour} 
        tourStep={tourStep}
        toggleLanguage={toggleLanguage}
        toggleTheme={toggleTheme}
        isDark={isDark}
      />

      {/* Main Content */}
      <main className="flex-1 flex flex-col h-screen overflow-hidden">
        <div className="flex-1 overflow-y-auto p-8">
          <div className="max-w-6xl mx-auto h-full">
            <AnimatePresence mode="wait">
              <motion.div
                key={activeView}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.2 }}
                className="h-full"
              >
                {activeView === 'dashboard' && <Dashboard />}
                {activeView === 'specs' && <SystemSpecs />}
                {activeView === 'profiles' && (
                  <Profiles 
                    onCustomize={(ids) => {
                      setSelectedTweaks(new Set(ids));
                      setActiveView('custom');
                    }}
                  />
                )}
                {activeView === 'custom' && (
                  <CustomTweaks 
                    selectedTweaks={selectedTweaks} 
                    setSelectedTweaks={setSelectedTweaks}
                  />
                )}
                {activeView === 'network' && <NetworkAnalysis />}
                {activeView === 'apps' && <AppInstaller />}
              </motion.div>
            </AnimatePresence>
          </div>
        </div>
      </main>

      {/* Global Undo */}
      <GlobalUndoWidget />
      <NpsPopup />

      {/* Onboarding Tour */}
      {showTour && <OnboardingTour onComplete={handleCompleteTour} onStepChange={setTourStep} />}
    </div>
    </>
  );
}
