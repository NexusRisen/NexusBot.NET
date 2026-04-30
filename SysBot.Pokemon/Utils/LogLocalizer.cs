using System;
using System.Collections.Generic;

namespace SysBot.Pokemon;

public static class LogLocalizer
{
    public static string CurrentLanguage { get; set; } = "en";

    private static readonly Dictionary<string, Dictionary<string, string>> LogTranslations = new()
    {
        ["zh-Hans"] = new()
        {
            ["Not Started"] = "未启动",
            ["Restarting the main loop."] = "正在重启主循环。",
            ["No task assigned. Waiting for new task assignment."] = "未分配任务。正在等待新任务分配。",
            ["Nothing to check, waiting for new users..."] = "没有可检查的内容，正在等待新用户...",
            ["Trying to reconnect..."] = "正在尝试重新连接...",
            ["Successfully recovered bot"] = "成功恢复机器人",
            ["Bot has stopped without error."] = "机器人已停止，无错误。",
            ["Bot has crashed!"] = "机器人已崩溃！",
            ["Starting next"] = "正在启动下一个",
            ["Bot Trade. Getting data..."] = "机器人交换。正在获取数据...",
            ["Saved file:"] = "已保存文件：",
            ["Config loaded and bots added:"] = "配置已加载，机器人已添加：",
            ["Controls loaded:"] = "控件已加载：",
            ["UI Initialization complete:"] = "UI 初始化完成：",
            ["Required folders created."] = "所需文件夹已创建。",
            ["Starting all bots..."] = "正在启动所有机器人...",
            ["Restarting all the consoles..."] = "正在重启所有控制台...",
            ["Detaching controllers on routine exit."] = "例程退出时正在分离控制器。",
            ["Closed out of the game!"] = "已关闭游戏！",
            ["Reconnecting to Y-Comm..."] = "正在重新连接到 Y-Comm...",
            ["Grabbing trainer data of host console..."] = "正在获取主机控制台的训练家数据...",
            ["Detaching on startup."] = "启动时正在分离。",
            ["Turning off screen."] = "正在关闭屏幕。",
            ["Turning on screen."] = "正在开启屏幕。",
            ["Potential soft ban detected, reopening game just in case!"] = "检测到潜在的软禁，以防万一重新开启游戏！",
            ["Destination slot is occupied! Dumping the Pokémon found there..."] = "目标位置已占用！正在转储在那里发现的宝可梦...",
            ["Clearing destination slot to start the bot."] = "正在清除目标位置以启动机器人。",
            ["Restarting the game!"] = "正在重启游戏！",
            ["Still not in the game, initiating rescue protocol!"] = "仍未进入游戏，正在启动救援协议！",
            ["Back in the overworld!"] = "回到大地图！",
            ["Soft ban detected, unbanning."] = "检测到软禁，正在解除。",
            ["Error detected, restarting the game!"] = "检测到错误，正在重启游戏！",
            ["Error detected, restarting the game!!"] = "检测到错误，正在重启游戏！！",
            ["Setting SV-specific hid waits"] = "正在设置 SV 特定 hid 等待",
            ["Initializing connection with console..."] = "正在初始化与控制台的连接...",
            ["Trainer data is not valid."] = "训练家数据无效。",
            ["Please remove interfering applications and reboot the Switch."] = "请移除干扰应用程序并重启 Switch。",
            ["minutes ago"] = "分钟前",
            ["Last traded with"] = "上次交换对象：",
            ["Found"] = "发现",
            ["ignoring the"] = "忽略了",
            ["minute trade cooldown."] = "分钟交换冷却时间。",
            ["using multiple accounts."] = "正在使用多个帐户。",
            ["Previously traded with"] = "之前曾与以下对象交换：",
            ["identified as"] = "识别为",
            ["using"] = "使用",
        },
        ["zh-Hant"] = new()
        {
            ["Not Started"] = "未啟動",
            ["Restarting the main loop."] = "正在重啟主迴圈。",
            ["No task assigned. Waiting for new task assignment."] = "未分配任務。正在等待新任務分配。",
            ["Nothing to check, waiting for new users..."] = "沒有可檢查內容，正在等待新用戶...",
            ["Trying to reconnect..."] = "正在嘗試重新連接...",
            ["Successfully recovered bot"] = "成功恢復機器人",
            ["Bot has stopped without error."] = "機器人已停止，無錯誤。",
            ["Bot has crashed!"] = "機器人已崩潰！",
            ["Starting next"] = "正在啟動下一個",
            ["Bot Trade. Getting data..."] = "機器人交換。正在獲取數據...",
            ["Saved file:"] = "已保存檔案：",
            ["Config loaded and bots added:"] = "配置已載入，機器人已添加：",
            ["Controls loaded:"] = "控制項已載入：",
            ["UI Initialization complete:"] = "UI 初始化完成：",
            ["Required folders created."] = "所需資料夾已創建。",
            ["Starting all bots..."] = "正在啟動所有機器人...",
            ["Restarting all the consoles..."] = "正在重啟所有控制台...",
        },
        ["fr"] = new()
        {
            ["Not Started"] = "Pas démarré",
            ["Restarting the main loop."] = "Redémarrage de la boucle principale.",
            ["No task assigned. Waiting for new task assignment."] = "Aucune tâche assignée. En attente d'une nouvelle tâche.",
            ["Nothing to check, waiting for new users..."] = "Rien à vérifier, en attente de nouveaux utilisateurs...",
            ["Trying to reconnect..."] = "Tentative de reconnexion...",
            ["Successfully recovered bot"] = "Robot récupéré avec succès",
            ["Bot has stopped without error."] = "Le robot s'est arrêté sans erreur.",
            ["Bot has crashed!"] = "Le robot a planté !",
            ["Starting next"] = "Démarrage du prochain",
            ["Bot Trade. Getting data..."] = "Échange de robot. Obtention des données...",
            ["Saved file:"] = "Fichier sauvegardé :",
            ["Required folders created."] = "Dossiers requis créés.",
            ["Starting all bots..."] = "Démarrage de tous les robots...",
            ["Restarting all the consoles..."] = "Redémarrage de toutes les consoles...",
        },
        ["de"] = new()
        {
            ["Not Started"] = "Nicht gestartet",
            ["Restarting the main loop."] = "Hauptschleife wird neu gestartet.",
            ["No task assigned. Waiting for new task assignment."] = "Keine Aufgabe zugewiesen. Warten auf neue Aufgabe.",
            ["Nothing to check, waiting for new users..."] = "Nichts zu prüfen, warten auf neue Benutzer...",
            ["Trying to reconnect..."] = "Versuche Verbindung wiederherzustellen...",
            ["Successfully recovered bot"] = "Bot erfolgreich wiederhergestellt",
            ["Bot has stopped without error."] = "Bot wurde ohne Fehler gestoppt.",
            ["Bot has crashed!"] = "Bot ist abgestürzt!",
            ["Starting next"] = "Starte nächsten",
            ["Bot Trade. Getting data..."] = "Bot-Tausch. Daten werden abgerufen...",
            ["Saved file:"] = "Datei gespeichert:",
            ["Required folders created."] = "Erforderliche Ordner erstellt.",
            ["Starting all bots..."] = "Starte alle Bots...",
            ["Restarting all the consoles..."] = "Starte alle Konsolen neu...",
        },
        ["ru"] = new()
        {
            ["Not Started"] = "Не запущен",
            ["Restarting the main loop."] = "Перезапуск основного цикла.",
            ["No task assigned. Waiting for new task assignment."] = "Задач нет. Ожидание назначения новой задачи.",
            ["Nothing to check, waiting for new users..."] = "Ничего не найдено, ожидание новых пользователей...",
            ["Trying to reconnect..."] = "Попытка переподключения...",
            ["Successfully recovered bot"] = "Бот успешно восстановлен",
            ["Bot has stopped without error."] = "Бот остановлен без ошибок.",
            ["Bot has crashed!"] = "Бот упал!",
            ["Starting next"] = "Запуск следующего",
            ["Bot Trade. Getting data..."] = "Обмен ботом. Получение данных...",
            ["Saved file:"] = "Файл сохранен:",
            ["Required folders created."] = "Необходимые папки созданы.",
            ["Starting all bots..."] = "Запуск всех ботов...",
            ["Restarting all the consoles..."] = "Перезагрузка всех консолей...",
        },
        ["es"] = new()
        {
            ["Not Started"] = "No iniciado",
            ["Restarting the main loop."] = "Reiniciando el bucle principal.",
            ["No task assigned. Waiting for new task assignment."] = "Sin tarea asignada. Esperando nueva tarea.",
            ["Nothing to check, waiting for new users..."] = "Nada que comprobar, esperando nuevos usuarios...",
            ["Trying to reconnect..."] = "Intentando reconectar...",
            ["Successfully recovered bot"] = "Bot recuperado con éxito",
            ["Bot has stopped without error."] = "El bot se ha detenido sin errores.",
            ["Bot has crashed!"] = "¡El bot se ha colgado!",
            ["Starting next"] = "Iniciando siguiente",
            ["Bot Trade. Getting data..."] = "Intercambio del bot. Obteniendo datos...",
            ["Saved file:"] = "Archivo guardado:",
            ["Required folders created."] = "Carpetas requeridas creadas.",
            ["Starting all bots..."] = "Iniciando todos los bots...",
            ["Restarting all the consoles..."] = "Reiniciando todas las consolas...",
        },
        ["it"] = new()
        {
            ["Not Started"] = "Non avviato",
            ["Restarting the main loop."] = "Riavvio del ciclo principale.",
            ["No task assigned. Waiting for new task assignment."] = "Nessun compito assegnato. In attesa di un nuovo compito.",
            ["Nothing to check, waiting for new users..."] = "Nulla da controllare, in attesa di nuovi utenti...",
            ["Trying to reconnect..."] = "Tentativo di riconnessione...",
            ["Successfully recovered bot"] = "Bot recuperato con successo",
            ["Bot has stopped without error."] = "Il bot si è fermato senza errori.",
            ["Bot has crashed!"] = "Il bot è andato in crash!",
            ["Starting next"] = "Avvio del prossimo",
            ["Bot Trade. Getting data..."] = "Scambio del bot. Ottenimento dati...",
            ["Saved file:"] = "File salvato:",
            ["Required folders created."] = "Cartelle richieste create.",
            ["Starting all bots..."] = "Avvio di tutti i bot...",
            ["Restarting all the consoles..."] = "Riavvio di tutte le console...",
        },
        ["ja"] = new()
        {
            ["Not Started"] = "未起動",
            ["Restarting the main loop."] = "メインループを再起動しています。",
            ["No task assigned. Waiting for new task assignment."] = "タスクが割り当てられていません。新しいタスクを待機中。",
            ["Nothing to check, waiting for new users..."] = "チェック対象なし、新しいユーザーを待機中...",
            ["Trying to reconnect..."] = "再接続を試みています...",
            ["Successfully recovered bot"] = "ボットの復旧に成功しました",
            ["Bot has stopped without error."] = "ボットはエラーなしで停止しました。",
            ["Bot has crashed!"] = "ボットがクラッシュしました！",
            ["Starting next"] = "次を開始しています",
            ["Bot Trade. Getting data..."] = "ボットトレード。データを取得中...",
            ["Saved file:"] = "ファイルを保存しました：",
            ["Required folders created."] = "必要なフォルダを作成しました。",
            ["Starting all bots..."] = "すべてのボットを開始しています...",
            ["Restarting all the consoles..."] = "すべてのコンソールを再起動しています...",
        },
        ["ko"] = new()
        {
            ["Not Started"] = "시작되지 않음",
            ["Restarting the main loop."] = "메인 루프를 재시작하는 중입니다.",
            ["No task assigned. Waiting for new task assignment."] = "할당된 작업이 없습니다. 새 작업을 기다리는 중.",
            ["Nothing to check, waiting for new users..."] = "확인할 내용 없음, 새 사용자를 기다리는 중...",
            ["Trying to reconnect..."] = "재연결을 시도 중입니다...",
            ["Successfully recovered bot"] = "봇 복구 성공",
            ["Bot has stopped without error."] = "봇이 오류 없이 중지되었습니다.",
            ["Bot has crashed!"] = "봇이 충돌했습니다!",
            ["Starting next"] = "다음 작업을 시작합니다",
            ["Bot Trade. Getting data..."] = "봇 트레이드. 데이터를 가져오는 중...",
            ["Saved file:"] = "파일 저장됨:",
            ["Required folders created."] = "필요한 폴더가 생성되었습니다.",
            ["Starting all bots..."] = "모든 봇을 시작하는 중...",
            ["Restarting all the consoles..."] = "모든 콘솔을 재시작하는 중...",
        }
    };

    public static string Translate(string message)
    {
        if (CurrentLanguage == "en")
            return message;

        if (!LogTranslations.TryGetValue(CurrentLanguage, out var langDict))
            return message;

        // Try direct match
        if (langDict.TryGetValue(message, out var translated))
            return translated;

        // Try fuzzy/partial matches
        foreach (var entry in langDict)
        {
            if (message.Contains(entry.Key))
            {
                message = message.Replace(entry.Key, entry.Value);
            }
        }

        return message;
    }
}
