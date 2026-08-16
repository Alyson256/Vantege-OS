import React, { useEffect, useState } from 'react';
import { Cpu, HardDrive, MemoryStick, Activity, CircuitBoard, Thermometer, ChevronDown, ChevronUp, Zap, Info } from 'lucide-react';
import { cn } from '../lib/utils';
import { Area, AreaChart, ResponsiveContainer, Tooltip as RechartsTooltip, YAxis } from 'recharts';
import { motion, AnimatePresence } from 'motion/react';
import { Tooltip } from './Tooltip';
import { useLanguage } from '../contexts/LanguageContext';

interface Stat {
  name: string;
  value: number | null;
  unit: string;
  temp?: number | null;
  icon: React.ElementType;
  color: string;
  tooltipKey: string;
}

const generateData = () => Array.from({ length: 20 }, (_, i) => ({
  time: i,
  cpu: 0,
  gpu: 0,
  ram: 0,
}));

export function HardwareMonitor() {
  const [data, setData] = useState(generateData());
  const [showChart, setShowChart] = useState(false);
  const { t } = useLanguage();
  const [stats, setStats] = useState<Stat[]>([
    { name: 'CPU', value: null, unit: '%', temp: null, icon: Cpu, color: 'text-blue-500', tooltipKey: 'tooltipCpu' },
    { name: 'GPU', value: null, unit: '%', temp: null, icon: CircuitBoard, color: 'text-rose-500', tooltipKey: 'tooltipGpu' },
    { name: 'RAM', value: null, unit: '%', icon: MemoryStick, color: 'text-purple-500', tooltipKey: 'tooltipRam' },
    { name: 'Disk', value: null, unit: '%', icon: HardDrive, color: 'text-emerald-500', tooltipKey: 'tooltipDisk' },
    { name: 'Energy', value: null, unit: 'W', icon: Zap, color: 'text-yellow-500', tooltipKey: 'tooltipEnergy' },
  ]);

  useEffect(() => {
    // Awaiting backend connection
  }, []);

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-5 gap-4">
        {stats.map((stat) => (
          <div key={stat.name} className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-4 flex items-center justify-between shadow-sm relative group">
            <div>
              <Tooltip content={t(stat.tooltipKey as any) as string} position="top" className="mb-1 w-max">
                <div className="flex items-center gap-1.5 cursor-help">
                  <p className="text-sm font-medium text-zinc-500 dark:text-zinc-400">
                    {stat.name === 'Energy' ? t('energy') : stat.name}
                  </p>
                  <Info className="w-3.5 h-3.5 text-zinc-400 dark:text-zinc-500" />
                </div>
              </Tooltip>
              
              <div className="flex items-baseline gap-1">
                <p className="text-2xl font-semibold text-zinc-900 dark:text-zinc-100">
                  {stat.value !== null ? stat.value : '--'}<span className="text-sm font-normal text-zinc-500 ml-1">{stat.unit}</span>
                </p>
              </div>
              {stat.temp !== undefined && (
                <div className="flex items-center gap-1 mt-1 text-xs font-medium text-zinc-400 dark:text-zinc-500">
                  <Thermometer className="w-3 h-3" />
                  {stat.temp !== null ? stat.temp + '°C' : '--'}
                </div>
              )}
            </div>
            <div className={cn("p-3 rounded-xl bg-zinc-100 dark:bg-zinc-800 flex-shrink-0", stat.color)}>
              <stat.icon className="w-5 h-5" />
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-6 shadow-sm">
        <div 
          className="flex items-center justify-between cursor-pointer group"
          onClick={() => setShowChart(!showChart)}
        >
          <h3 className="text-lg font-medium text-zinc-900 dark:text-zinc-100">{t('sysUse')}</h3>
          <button className="p-2 rounded-xl bg-zinc-100 dark:bg-zinc-800 text-zinc-500 dark:text-zinc-400 group-hover:bg-zinc-200 dark:group-hover:bg-zinc-700 transition-colors">
            {showChart ? <ChevronUp className="w-5 h-5" /> : <ChevronDown className="w-5 h-5" />}
          </button>
        </div>
        
        <AnimatePresence>
          {showChart && (
            <motion.div 
              initial={{ height: 0, opacity: 0, marginTop: 0 }}
              animate={{ height: 256, opacity: 1, marginTop: 24 }}
              exit={{ height: 0, opacity: 0, marginTop: 0 }}
              className="w-full overflow-hidden"
            >
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={data} margin={{ top: 0, right: 0, left: -20, bottom: 0 }}>
                  <defs>
                    <linearGradient id="colorCpu" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3}/>
                      <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                    </linearGradient>
                    <linearGradient id="colorGpu" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#f43f5e" stopOpacity={0.3}/>
                      <stop offset="95%" stopColor="#f43f5e" stopOpacity={0}/>
                    </linearGradient>
                    <linearGradient id="colorRam" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#a855f7" stopOpacity={0.3}/>
                      <stop offset="95%" stopColor="#a855f7" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <YAxis tick={{ fill: '#71717a', fontSize: 12 }} axisLine={false} tickLine={false} />
                  <RechartsTooltip 
                    contentStyle={{ backgroundColor: 'rgba(24, 24, 27, 0.9)', border: 'none', borderRadius: '12px', color: '#fff' }}
                    itemStyle={{ color: '#e4e4e7' }}
                  />
                  <Area type="monotone" dataKey="cpu" stroke="#3b82f6" strokeWidth={2} fillOpacity={1} fill="url(#colorCpu)" name="CPU %" />
                  <Area type="monotone" dataKey="gpu" stroke="#f43f5e" strokeWidth={2} fillOpacity={1} fill="url(#colorGpu)" name="GPU %" />
                  <Area type="monotone" dataKey="ram" stroke="#a855f7" strokeWidth={2} fillOpacity={1} fill="url(#colorRam)" name="RAM %" />
                </AreaChart>
              </ResponsiveContainer>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
