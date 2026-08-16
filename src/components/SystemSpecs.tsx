import React from 'react';
import { Cpu, CircuitBoard, MemoryStick, Monitor, HardDrive, MonitorCheck, Info } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';
import { motion } from 'motion/react';

export function SystemSpecs() {
  const { t } = useLanguage();

  const specs = [
    {
      category: t('processor'),
      icon: Cpu,
      color: 'text-blue-500',
      bgColor: 'bg-blue-50 dark:bg-blue-500/10',
      items: [
        { label: t('lblModel'), value: t('waitingData') },
        { label: t('lblCores'), value: t('waitingData') },
        { label: t('lblBaseClock'), value: t('waitingData') },
        { label: t('lblBoostClock'), value: t('waitingData') },
        { label: t('lblCacheL3'), value: t('waitingData') },
      ]
    },
    {
      category: t('graphics'),
      icon: Monitor,
      color: 'text-rose-500',
      bgColor: 'bg-rose-50 dark:bg-rose-500/10',
      items: [
        { label: t('lblModel'), value: t('waitingData') },
        { label: t('lblVram'), value: t('waitingData') },
        { label: t('lblDriver'), value: t('waitingData') },
        { label: t('lblDirectx'), value: t('waitingData') },
      ]
    },
    {
      category: t('motherboard'),
      icon: CircuitBoard,
      color: 'text-emerald-500',
      bgColor: 'bg-emerald-50 dark:bg-emerald-500/10',
      items: [
        { label: t('lblManufacturer'), value: t('waitingData') },
        { label: t('lblModel'), value: t('waitingData') },
        { label: t('lblChipset'), value: t('waitingData') },
        { label: t('lblBiosVersion'), value: t('waitingData') },
      ]
    },
    {
      category: t('memory'),
      icon: MemoryStick,
      color: 'text-purple-500',
      bgColor: 'bg-purple-50 dark:bg-purple-500/10',
      items: [
        { label: t('lblTotalCapacity'), value: t('waitingData') },
        { label: t('lblSpeed'), value: t('waitingData') },
        { label: t('lblType'), value: t('waitingData') },
        { label: t('lblSlotsUsed'), value: t('waitingData') },
      ]
    },
    {
      category: t('storage'),
      icon: HardDrive,
      color: 'text-amber-500',
      bgColor: 'bg-amber-50 dark:bg-amber-500/10',
      items: [
        { label: t('lblDisk1'), value: t('waitingData') },
        { label: t('lblDisk2'), value: t('waitingData') },
        { label: t('lblHealth'), value: t('waitingData') },
        { label: t('lblPartitionType'), value: t('waitingData') },
      ]
    },
    {
      category: t('os'),
      icon: MonitorCheck,
      color: 'text-cyan-500',
      bgColor: 'bg-cyan-50 dark:bg-cyan-500/10',
      items: [
        { label: t('lblEdition'), value: t('waitingData') },
        { label: t('lblVersion'), value: t('waitingData') },
        { label: t('lblArchitecture'), value: t('waitingData') },
        { label: t('lblSecureBoot'), value: t('waitingData') },
      ]
    }
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100">
            {t('systemSpecs')}
          </h2>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
            {t('systemSpecsDesc')}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        {specs.map((spec, index) => (
          <motion.div
            key={spec.category}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
            className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 mb-5">
              <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${spec.bgColor}`}>
                <spec.icon className={`w-5 h-5 ${spec.color}`} />
              </div>
              <h3 className="font-semibold text-zinc-900 dark:text-zinc-100">
                {spec.category}
              </h3>
            </div>
            
            <div className="space-y-3">
              {spec.items.map((item) => (
                <div key={item.label} className="flex flex-col">
                  <span className="text-xs font-medium text-zinc-500 dark:text-zinc-500 mb-0.5">
                    {item.label}
                  </span>
                  <span className="text-sm font-medium text-zinc-900 dark:text-zinc-100 truncate">
                    {item.value}
                  </span>
                </div>
              ))}
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
