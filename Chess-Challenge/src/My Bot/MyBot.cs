using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    private int CurrTurn = 0;
    private int AlphaBetaStart = 0;

    public Move Think(Board board, Timer timer)
    {
        CurrTurn++;
        Move move;
        try
        {
            (move, _, _) = CalculateMove(board, timer, CurrTurn < AlphaBetaStart ? 3 : 3,
                (Int64.MinValue, Int64.MinValue));
        }
        catch (Exception e)
        {
            return board.GetLegalMoves()[0];
        }

        return move;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    public (Int64, Int64) UpdateAlphaBeta(Board board, Int64 result, (Int64, Int64) alphaBeta)
    {
        if (CurrTurn < AlphaBetaStart)
        {
            return alphaBeta;
        }

        if (board.IsWhiteToMove)
        {
            if (alphaBeta.Item1 < result)
            {
                alphaBeta.Item1 = result;
            }
        }
        else
        {
            if (alphaBeta.Item2 < result)
            {
                alphaBeta.Item2 = result;
            }
        }

        return alphaBeta;
    }

    public Move[] combineMove(Move[] capturing, Move[] moves) //Optimize
    {
        if (CurrTurn < AlphaBetaStart)
        {
            return moves;
        }

        foreach (var currMove in moves)
        {
            bool notCapturing = true;
            foreach (var capturingMove in capturing)
            {
                if (currMove == capturingMove)
                {
                    notCapturing = false;
                    break;
                }
            }

            if (notCapturing)
            {
                capturing.Append(currMove);
            }
        }

        return capturing;
    }

    public (Move, Int64, (Int64, Int64)) CalculateMove(Board board, Timer timer, int depth, (Int64, Int64) alphaBeta)
    {
        Int64 alpha = board.IsWhiteToMove ? alphaBeta.Item1 : alphaBeta.Item2;

        if (depth < 1)
        {
            Int64 result = EvaluateBoard(board);
            return (new Move(), result, UpdateAlphaBeta(board, result, alphaBeta));
        }

        Move[] allMoves = board.GetLegalMoves();
        if (allMoves.Length <= 0)
        {
            return (new Move(), 0, UpdateAlphaBeta(board, 0, alphaBeta));
        }

        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        Int64 highestValueMove = Int64.MinValue;
        allMoves = combineMove(board.GetLegalMoves(true), allMoves);

        foreach (Move move in allMoves)
        {
            Int64 boardVal;

            if (MoveIsCheckmate(board, move))
            {
                return (move, Int64.MaxValue, UpdateAlphaBeta(board, Int64.MaxValue, alphaBeta));
            }

            board.MakeMove(move);
            (_, boardVal, alphaBeta) = CalculateMove(board, timer, depth - 1, alphaBeta);
            boardVal *= -1;

            //Alpha beta pruning
            if (alpha > boardVal && CurrTurn >= AlphaBetaStart)
            {
                board.UndoMove(move);
                return (moveToPlay, highestValueMove, alphaBeta);
            }

            board.UndoMove(move);

            if (boardVal > highestValueMove)
            {
                moveToPlay = move;
                highestValueMove = boardVal;
            }
        }

        return (moveToPlay, highestValueMove, UpdateAlphaBeta(board, highestValueMove, alphaBeta));
    }

    public Int64 EvaluateBoard(Board board)
    {
        Int64 boardVal = 0;
        foreach (var currentPieceList in board.GetAllPieceLists())
        {
            int isWhite = currentPieceList.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1;
            int currPieceCount = currentPieceList.Count;
            PieceType currPieceType = currentPieceList.TypeOfPieceInList;
            int currPieceValue = pieceValues[(int)currPieceType];

            boardVal += isWhite * currPieceValue * currPieceCount;
        }

        return boardVal;
    }
}