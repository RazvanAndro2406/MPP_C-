import sqlite3
db = r"C:\\Facultate\\Anul_2\\Semestru_2\\MPP\\C#\\Lab2\\identifier.sqlite"
con = sqlite3.connect(db)
cur = con.cursor()
print(cur.execute("select name from sqlite_master where type='table' order by name").fetchall())
for (t,) in cur.execute("select name from sqlite_master where type='table' and name not like 'sqlite_%'").fetchall():
    print('---', t)
    print(cur.execute(f'pragma table_info({t})').fetchall())
