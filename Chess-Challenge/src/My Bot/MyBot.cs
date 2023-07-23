using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    private int CurrTurn = 0;
    private int HighDepthStart = 5;

    public Move Think(Board board, Timer timer)
    {
        CurrTurn++;
        Move move;
        try
        {
            (move, _) = CalculateMove(board, timer, CurrTurn>HighDepthStart ? 4:6);
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

    public Move[] CombineMoves(Move[] legalMoves, Move[] capturingMoves)
    {
        List<Move> combinedMoves = new List<Move>(capturingMoves);
        foreach (Move move in legalMoves)
        {
            if (!combinedMoves.Contains(move))
            {
                combinedMoves.Add(move);
            }
        }
        return combinedMoves.ToArray();
    }




    public (Move, Int64) CalculateMove(Board board, Timer timer, int depth, Int64 alpha = Int64.MinValue,
        Int64 beta = Int64.MaxValue)
    {
        if (depth < 1)
        {
            return (new Move(), EvaluateBoard(board));
        }

        Move[] allMoves = board.GetLegalMoves();
        if (allMoves.Length <= 0)
        {
            return (new Move(), 0);
        }

        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        Int64 highestValueMove = Int64.MinValue;
        allMoves = CombineMoves(allMoves, board.GetLegalMoves(true));

        foreach (Move move in allMoves)
        {
            Int64 boardVal;

            if (MoveIsCheckmate(board, move))
            {
                return (move, Int64.MaxValue);
            }

            board.MakeMove(move);
            (_, boardVal) = CalculateMove(board, timer, depth - 1, alpha, beta);
            boardVal *= -1;
            board.UndoMove(move);

            if (boardVal > highestValueMove)
            {
                moveToPlay = move;
                highestValueMove = boardVal;
            }

            if (board.IsWhiteToMove)
            {
                alpha = Math.Max(alpha, highestValueMove);
                if (alpha >= beta)
                    break;
            }
            else
            {
                beta = Math.Min(beta, -highestValueMove);
                if (alpha >= beta)
                    break;
            }
        }

        return (moveToPlay, highestValueMove);
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