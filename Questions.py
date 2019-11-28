def q1():
    text = """
    I'm Alexi. I love solving difficult problems. I enjoy Professor Layton games, programming, and chess. I have now turned my passion into a business:

    Do you have a difficult problem you cannot solve, need an algorithm for a particularly troublesome application you are writing or simply want an app made for you? Send me a direct message, 
    detailing the job you would like me to do for you, and I will let you know, by return, if I can help.

    I charge for each task individually. Prices depend on how much time it will take me and how enjoyable the job sounds. Remember, I enjoy difficult problems rather than easy ones! If you are not 
    sure what kind of problems I enjoy solving, you are already in the right place. Read my blog entries below where I discuss some of the more interesting tasks I have worked on.

    I look forward to hearing from you!"""

    answer = 0

    for char in text:
        char = char.lower()
        if char.isalpha():
            answer += ord(char) - (ord('a') - 1)

    print(answer)

def q2():
    data = ['2010','2001','0201','0210','0021','0021','1020','2100','1002','2010','0021','0210','1002','0201','2010','2001','0201','0210','0021','0021','1020','2100','1002','2010','0021','0210','1002',
    '0201','0012','0210','0201','1002','1002','1200','0120','1020','1020','0201','0012','2100','0021','0102','0210','0012','2010','2001','2100','2100','1020','2010','2100','0012','2100','2001','2010',
    '2001','0201','0210','0021','0021','1020','2100','1002','2010','0021','0210','1002','0201','2010','2001','0201','0210','0021','0021','1020','2100','1002','2010','0021','0210','1002','0201','0012',
    '0210','0201','1002','1002','1200','0120','1020','1020','0201','0012','2100','0021','0102','0210','0012','2010','2001','2100','2100','1020','2010','2100','0012','2100','2001','2001','2001','0021',
    '0120','1020','0201','0012','2001','2100','0102','0021','1020','2100','1002','2001','0012','0201','2001','1020','0012','0210','0201','1002','1002','1200','0120','1020','1020','0201','0012','2100',
    '0021','0102','0210','0012','2010','2001','2100','2100','1020','2010','2100','0012','2100','2001','2001','2001','0021','0120','1020','0201','0012','2001','2100','0102','0021','1020','2100','1002',
    '2001','0012','0201','2001','1020','2001','2001','0021','0120','1020','0201','0012','2001','2100','0102','0021','1020','2100','1002','2001','0012','0201','2001','1020''0012','0210','0201','1002',
    '1002','1200','0120','1020','1020','0201','0012','2100','0021','0102','0210','0012','2010','2001','2100','2100','1020','2010','2100','0012','2100','2001','2001','2001','0021','0120','1020','0201',
    '0012','2001','2100','0102','0021','1020','2100','1002','2001','0012','0201','2001','1020']

    votes = [0, 0, 0, 0]
    for vote in data:
        votes[vote.index("1")] += 1

    ind = votes.index(min(votes))

    for vote in data:
        if vote.index("1") == ind:
            votes[vote.index("2")] += 1

    print(max(votes))

q1()
q2()
