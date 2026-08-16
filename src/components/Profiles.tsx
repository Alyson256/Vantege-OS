import React, { useState, useEffect } from 'react';
import { Gamepad2, ShieldCheck, User, CheckCircle, Pencil } from 'lucide-react';
import { cn } from '../lib/utils';
import { motion } from 'motion/react';
import { ConfirmationModal } from './ConfirmationModal';
import { TWEAKS_DB } from '../data/tweaks.data';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';

interface ProfilesProps {
  onCustomize: (tweaks: string[]) => void;
}

export function Profiles({ onCustomize }: ProfilesProps) {
  const { t } = useLanguage();
  const { registerAction } = useActionHistory();
  const [activeProfile, setActiveProfile] = useState<string>('gaming');
  const [applying, setApplying] = useState(false);
  const [modalState, setModalState] = useState<{ isOpen: boolean; profileId: string | null }>({ isOpen: false, profileId: null });
  const [applyingId, setApplyingId] = useState<string | null>(null);

  const [customSlots, setCustomSlots] = useState<{
    [key: number]: { tweaks: string[]; name: string };
  }>({
    1: { tweaks: [], name: '' },
    2: { tweaks: [], name: '' },
    3: { tweaks: [], name: '' }
  });

  useEffect(() => {
    const slots = { ...customSlots };
    for (let i = 1; i <= 3; i++) {
      try {
        const savedTweaks = localStorage.getItem(`customTweaks_${i}`);
        const savedName = localStorage.getItem(`customProfileName_${i}`);
        if (savedTweaks) {
          slots[i as keyof typeof slots].tweaks = JSON.parse(savedTweaks);
        }
        if (savedName) {
          slots[i as keyof typeof slots].name = savedName;
        }
      } catch (e) {}
    }
    setCustomSlots(slots);
  }, []);

  const emptySlots = [1, 2, 3].filter(slot => customSlots[slot as 1|2|3].tweaks.length === 0);
  const firstEmptySlot = emptySlots.length > 0 ? emptySlots[0] : null;
  const visibleCustomSlots = [1, 2, 3].filter(slot => customSlots[slot as 1|2|3].tweaks.length > 0 || slot === firstEmptySlot);

  const PROFILES = [
    {
      id: 'gaming',
      name: t('profGamer'),
      description: t('profGamerDesc'),
      icon: Gamepad2,
      color: 'text-rose-500',
      bg: 'bg-rose-500/10',
      border: 'border-rose-500/20',
      hoverBorder: 'hover:border-rose-500/50',
    },
    {
      id: 'checkup',
      name: t('profCheck'),
      description: t('profCheckDesc'),
      icon: ShieldCheck,
      color: 'text-emerald-500',
      bg: 'bg-emerald-500/10',
      border: 'border-emerald-500/20',
      hoverBorder: 'hover:border-emerald-500/50',
    },
    ...visibleCustomSlots.map((slot: number) => ({
      id: `custom_${slot}`,
      name: customSlots[slot as 1|2|3].name || `${t('myProfile')} ${slot}`,
      description: customSlots[slot as 1|2|3].tweaks.length > 0 ? t('myProfileDesc') : t('emptyProfile'),
      icon: User,
      color: 'text-purple-500',
      bg: 'bg-purple-500/10',
      border: 'border-purple-500/20',
      hoverBorder: 'hover:border-purple-500/50',
      isCustom: true,
      slot: slot,
      tweaks: customSlots[slot as 1|2|3].tweaks
    }))
  ];

  const handleProfileClick = (id: string, tweaks?: string[]) => {
    if (id.startsWith('custom') && (!tweaks || tweaks.length === 0)) return;
    setModalState({ isOpen: true, profileId: id });
  };

  const getTweaksForProfile = (profileId: string) => {
    if (profileId.startsWith('custom_')) {
      const slot = parseInt(profileId.split('_')[1]);
      return customSlots[slot as 1|2|3].tweaks;
    }
    const targetCategory = profileId === 'gaming' ? 'Gaming' : 'Checkup';
    return TWEAKS_DB.filter(t => t.recommendedFor.includes(targetCategory as any)).map(t => t.id);
  };

  const confirmApplyProfile = () => {
    const id = modalState.profileId;
    setModalState({ isOpen: false, profileId: null });
    if (!id) return;
    
    setApplyingId(id);
    setApplying(true);
    
    setTimeout(() => {
      setApplying(false);
      setApplyingId(null);
      setActiveProfile(id);
      
      const profile = PROFILES.find(p => p.id === id);
      registerAction({
        type: 'optimization',
        title: `${t('applyProf')}: ${profile?.name}`
      });
    }, 2000);
  };

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      <div>
        <h2 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">{t('profTitle')}</h2>
        <p className="text-zinc-500 dark:text-zinc-400 mt-1">{t('profDesc')}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        {PROFILES.map((profile) => (
          <div 
            key={profile.id}
            className={cn(
              "relative overflow-hidden bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border rounded-3xl p-6 transition-all duration-300 flex flex-col h-full",
              profile.border, profile.hoverBorder,
              activeProfile === profile.id ? "ring-2 ring-offset-2 ring-offset-zinc-50 dark:ring-offset-black " + (profile.id === 'gaming' ? 'ring-rose-500' : profile.id === 'checkup' ? 'ring-emerald-500' : 'ring-purple-500') : ''
            )}
          >
            {activeProfile === profile.id && (
              <div className="absolute top-6 right-6">
                <CheckCircle className={cn("w-6 h-6", profile.color)} />
              </div>
            )}
            <div className={cn("w-14 h-14 rounded-2xl flex items-center justify-center mb-6", profile.bg)}>
              <profile.icon className={cn(
                "w-7 h-7", 
                profile.color,
                activeProfile === profile.id && profile.id === 'gaming' ? 'animate-pulse drop-shadow-md' : ''
              )} />
            </div>
            
            <h3 className="text-xl font-semibold text-zinc-900 dark:text-zinc-100 mb-2 flex items-center gap-2">
              <span className="truncate">{profile.name}</span>
              {(profile as any).isCustom && (profile as any).tweaks.length > 0 && (
                <span className="text-xs bg-purple-100 dark:bg-purple-500/20 text-purple-600 dark:text-purple-400 px-2 py-1 rounded-full font-medium whitespace-nowrap">
                  {(profile as any).tweaks.length} scripts
                </span>
              )}
            </h3>
            
            <p className="text-zinc-500 dark:text-zinc-400 text-sm leading-relaxed mb-8 flex-1">
              {profile.description}
            </p>
            
            <div className="flex flex-col gap-3 mt-auto">
              <button
                onClick={() => handleProfileClick(profile.id, (profile as any).tweaks)}
                disabled={applying || activeProfile === profile.id || ((profile as any).isCustom && (profile as any).tweaks.length === 0)}
                className="w-full py-3 px-4 rounded-xl font-medium bg-zinc-100 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-100 hover:bg-zinc-200 dark:hover:bg-zinc-700 transition-colors disabled:opacity-50 flex justify-center items-center h-12"
              >
                {applyingId === profile.id ? (
                   <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex items-center gap-2">
                     <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                     {t('applying')}
                   </motion.div>
                 ) : applying ? t('applying') : activeProfile === profile.id ? t('activeProf') : t('applyProf')}
              </button>
              <button
                onClick={() => onCustomize(getTweaksForProfile(profile.id))}
                disabled={applying || ((profile as any).isCustom && (profile as any).tweaks.length === 0)}
                className="w-full py-3 px-4 rounded-xl font-medium border-2 border-zinc-200 dark:border-zinc-800 text-zinc-700 dark:text-zinc-300 hover:border-zinc-300 dark:hover:border-zinc-700 transition-colors disabled:opacity-50 flex justify-center items-center h-12"
              >
                {t('viewCustomize')}
              </button>
            </div>
          </div>
        ))}
      </div>

      <ConfirmationModal
        isOpen={modalState.isOpen}
        title={t('applyProf')}
        description={t('modalProfDesc')}
        onConfirm={confirmApplyProfile}
        onCancel={() => setModalState({ isOpen: false, profileId: null })}
        confirmText={t('confirmApply')}
        cancelText={t('cancel')}
        isDestructive={modalState.profileId === 'gaming'}
      />
    </div>
  );
}
