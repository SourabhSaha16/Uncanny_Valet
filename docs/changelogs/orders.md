# Orders changelog

Entries are added by the orders agent, one per commit. Format in docs/changelogs/README.md.

## orders: add boss order module with visit history and strikes
Date: 2026-11-07
Files: Assets/_Project/Scripts/Orders/*.cs and UncannyValet.Orders.asmdef (OrderBook, OrderSelector, OrderScheduler, OrderJudge, CarVisitHistory, ParkedCars, StrikeTracker, OrderTracker, OrderIssuer, OrderResolver, BossOrderDirector, OrdersConfig), Assets/_Project/Data/Orders/OrdersConfig.asset, Assets/_Project/Tests/EditMode/Orders/*.cs, Assets/_Project/Tests/EditMode/UncannyValet.Tests.EditMode.asmdef (added UncannyValet.Orders reference)
Why: Boss messages (design.md section 6, pillar 2). From night 2 the boss orders a parked car into a free regular spot; the order is judged only when the car is delivered (visited the target spot after the order = pass, else fail and a strike); any other ticket end cancels it with no strike. Strikes are counted across the run by one StrikeTracker and reset when night 1 starts again.
Tests: 34 NUnit tests under Tests/EditMode/Orders (rules, issue flow, resolve flow, strikes). Run outside Unity against UnityEngine stubs: 34 passed. Not run in the Unity editor.
