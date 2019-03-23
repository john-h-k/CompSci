import math
import copy

ran = int(input())
num = int(input())
beacons = [[True, (0, 0)]]

for i in range(num):
    x, y = input().split(" ")
    # because it is a list of lists, we need to deepcopy not copy later !!
    beacons.append([False, (int(x), int(y))])

prev = -1

while True:
    # copy so we don't light beacons that were lit this round
    tmp = copy.deepcopy(beacons)
    for i, b in enumerate(beacons):
        for oi, ob in enumerate(beacons):
            dist = math.hypot(b[1][0] - ob[1][0], b[1][1] - ob[1][1])
            # if one of them is true and they are in distance, both are true
            if b[0] != ob[0] and dist <= ran:                
                tmp[i][0] = True
                tmp[oi][0] = True
                break

            
    c = 0
    for b in tmp:
        if b[0] and b != [True, (0, 0)]:
            c += 1

    # if no new beacons lit, break
    if prev == c:
        break
            
    prev = c
    beacons = tmp
    print(c)
